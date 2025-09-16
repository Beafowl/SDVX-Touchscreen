using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using Device = SharpDX.Direct3D11.Device;

    public class FrameCapturedEventArgs : EventArgs
    {
        public Bitmap FrameBitmap { get; }
        public FrameCapturedEventArgs(Bitmap bmp) => FrameBitmap = bmp;
    }

    public class MyScreenCapturer : IDisposable
    {
        public event EventHandler<FrameCapturedEventArgs> FrameCaptured;

        private readonly int adapterIndex;
        private readonly int outputIndex;
        private Device device;
        private OutputDuplication duplicator;
        private Texture2D stagingTexture;
        private CancellationTokenSource cts;
        private Task captureTask;
        private int width;
        private int height;

        public MyScreenCapturer(int adapterIndex = 0, int outputIndex = 0)
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
            device = new Device(adapter);

            using (var output = adapter.GetOutput(outputIndex))
            using (var output1 = output.QueryInterface<Output1>())
            {
                duplicator = output1.DuplicateOutput(device);
                var bounds = output.Description.DesktopBounds;
                width = bounds.Right - bounds.Left;
                height = bounds.Bottom - bounds.Top;
            }

            // 2) Create a staging texture for CPU readback
            var desc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                ArraySize = 1,
                MipLevels = 1
            };
            stagingTexture = new Texture2D(device, desc);
        }

        public void StartCapture(int frameIntervalMs = 16)
        {
            if (captureTask != null) return;

            cts = new CancellationTokenSource();
            captureTask = Task.Run(() => CaptureLoop(frameIntervalMs, cts.Token), cts.Token);
        }

        public void StopCapture()
        {
            if (cts == null) return;
            cts.Cancel();
            captureTask.Wait();
            captureTask = null;
            cts.Dispose();
            cts = null;
        }

        public Bitmap CaptureFrame()
        {
            bool frameTaken = false;
            int frameIntervalMs = 16;


            while (!frameTaken)
            {
                try
                {
                    var dc = device.ImmediateContext;

                    // Acquire next frame
                    Result frameResult = duplicator.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);
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
                        dc.CopyResource(screenTex, stagingTexture);
                    }

                    // Release frame asap
                    duplicator.ReleaseFrame();
                    desktopResource.Dispose();

                    // Map and convert to Bitmap
                    var dataBox = dc.MapSubresource(
                        stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    var bmpData = bmp.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        bmp.PixelFormat);

                    IntPtr srcPtr = dataBox.DataPointer;
                    IntPtr dstPtr = bmpData.Scan0;
                    int srcStride = dataBox.RowPitch;
                    int dstStride = bmpData.Stride;
                    int rowBytes = Math.Min(srcStride, dstStride);

                    for (int y = 0; y < height; y++)
                    {
                        Utilities.CopyMemory(
                            dstPtr + y * dstStride,
                            srcPtr + y * srcStride,
                            rowBytes);
                    }

                    bmp.UnlockBits(bmpData);
                    dc.UnmapSubresource(stagingTexture, 0);

                    return bmp;
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
            throw new Exception("what");
        }

        private void CaptureLoop(int frameIntervalMs, CancellationToken token)
        {
            var dc = device.ImmediateContext;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Acquire next frame
                    Result frameResult = duplicator.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);
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
                        dc.CopyResource(screenTex, stagingTexture);
                    }

                    // Release frame asap
                    duplicator.ReleaseFrame();
                    desktopResource.Dispose();

                    // Map and convert to Bitmap
                    var dataBox = dc.MapSubresource(
                        stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    var bmpData = bmp.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        bmp.PixelFormat);

                    IntPtr srcPtr = dataBox.DataPointer;
                    IntPtr dstPtr = bmpData.Scan0;
                    int srcStride = dataBox.RowPitch;
                    int dstStride = bmpData.Stride;
                    int rowBytes = Math.Min(srcStride, dstStride);

                    for (int y = 0; y < height; y++)
                    {
                        Utilities.CopyMemory(
                            dstPtr + y * dstStride,
                            srcPtr + y * srcStride,
                            rowBytes);
                    }

                    bmp.UnlockBits(bmpData);
                    dc.UnmapSubresource(stagingTexture, 0);

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
            stagingTexture?.Dispose();
            duplicator?.Dispose();
            device?.Dispose();
        }
        private void RecoverFromDeviceRemoval()
        {
            // 1) Tear down
            duplicator?.Dispose();
            stagingTexture?.Dispose();
            device?.Dispose();

            // 2) Re-init
            InitializeDuplication();
        }
    }
}
