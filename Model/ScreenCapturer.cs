using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Drawing.Imaging;
using Device = SharpDX.Direct3D11.Device;

namespace Model
{


    public class FrameCapturedEventArgs : EventArgs
    {
        public Bitmap FrameBitmap { get; }
        public FrameCapturedEventArgs(Bitmap bmp) => FrameBitmap = bmp;
    }

    public class ScreenCapturer : IDisposable
    {
        public event EventHandler<FrameCapturedEventArgs> FrameCaptured;

        private readonly int adapterIndex;
        private readonly int outputIndex;
        private Device _device;
        private OutputDuplication _duplicator;
        private Texture2D _stagingTexture;
        private CancellationTokenSource _cts;
        private Task _captureTask;
        private int _width;
        private int _height;

        public ScreenCapturer(int adapterIndex = 0, int outputIndex = 0)
        {
            this.adapterIndex = adapterIndex;
            this.outputIndex = outputIndex;
            InitializeDuplication();
        }

        private void InitializeDuplication()
        {
            // 1) Create DXGI factory, adapter, device, and duplicator
            var factory = new Factory1();
            var adapter = factory.GetAdapter1(adapterIndex);
            _device = new Device(adapter);

            using (var output = adapter.GetOutput(outputIndex))
            using (var output1 = output.QueryInterface<Output1>())
            {
                _duplicator = output1.DuplicateOutput(_device);
                var bounds = output.Description.DesktopBounds;
                _width = bounds.Right - bounds.Left;
                _height = bounds.Bottom - bounds.Top;
            }

            // 2) Create a staging texture for CPU readback
            var desc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = _width,
                Height = _height,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                ArraySize = 1,
                MipLevels = 1
            };
            _stagingTexture = new Texture2D(_device, desc);
        }

        public void StartCapture(int frameIntervalMs = 16)
        {
            if (_captureTask != null) return;

            _cts = new CancellationTokenSource();
            _captureTask = Task.Run(() => CaptureLoop(frameIntervalMs, _cts.Token), _cts.Token);
        }

        public void StopCapture()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _captureTask.Wait();
            _captureTask = null;
            _cts.Dispose();
            _cts = null;
        }

        private void CaptureLoop(int frameIntervalMs, CancellationToken token)
        {
            var dc = _device.ImmediateContext;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Acquire next frame
                    Result frameResult = _duplicator.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);
                    if (frameResult != Result.Ok)
                    {
                        Console.WriteLine("Sleeping");
                        Thread.Sleep(10000);
                        InitializeDuplication();
                        continue;
                    }

                    // Copy to staging texture
                    using (var screenTex = desktopResource.QueryInterface<Texture2D>())
                    {
                        if (screenTex == null || screenTex.IsDisposed)
                            throw new InvalidOperationException("screenTex is null or disposed.");

                        if (_stagingTexture == null || _stagingTexture.IsDisposed)
                            throw new InvalidOperationException("stagingTexture is null or disposed.");

                        dc.CopySubresourceRegion(screenTex, 0, null, _stagingTexture, 0);
                        // Release frame asap
                        _duplicator.ReleaseFrame();
                        desktopResource.Dispose();
                    }



                    // Map and convert to Bitmap
                    var dataBox = dc.MapSubresource(
                        _stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                    var bmp = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                    var bmpData = bmp.LockBits(
                        new Rectangle(0, 0, _width, _height),
                        ImageLockMode.WriteOnly,
                        bmp.PixelFormat);

                    IntPtr srcPtr = dataBox.DataPointer;
                    IntPtr dstPtr = bmpData.Scan0;
                    int srcStride = dataBox.RowPitch;
                    int dstStride = bmpData.Stride;
                    int rowBytes = Math.Min(srcStride, dstStride);

                    for (int y = 0; y < _height; y++)
                    {
                        Utilities.CopyMemory(
                            dstPtr + y * dstStride,
                            srcPtr + y * srcStride,
                            rowBytes);
                    }

                    bmp.UnlockBits(bmpData);
                    dc.UnmapSubresource(_stagingTexture, 0);

                    // Fire event
                    FrameCaptured?.Invoke(this, new FrameCapturedEventArgs(bmp));
                }
                catch (SharpDXException sx) when (sx.ResultCode == SharpDX.DXGI.ResultCode.WaitTimeout)
                {
                    // No new frame this interval—ignore
                }
                catch (SharpDXException ex) when (ex.ResultCode == SharpDX.DXGI.ResultCode.AccessLost)
                {
                    // The desktop duplication was lost (e.g. exclusive fullscreen entered)
                    // Reinitialize duplicator & continue capturing
                    InitializeDuplication();
                }
                catch (Exception ex)
                {
                    // Unexpected error: break or log
                    Console.WriteLine("Capture error: " + ex);
                    RecoverFromDeviceRemoval();
                }

                Thread.Sleep(frameIntervalMs);
            }
        }

        public void Dispose()
        {
            StopCapture();
            _stagingTexture?.Dispose();
            _duplicator?.Dispose();
            _device?.Dispose();
        }
        private void RecoverFromDeviceRemoval()
        {
            // 1) Tear down
            _duplicator?.Dispose();
            _stagingTexture?.Dispose();
            _device?.Dispose();

            // 2) Re-init
            InitializeDuplication();
        }
    }
}
