using CommunityToolkit.Mvvm.ComponentModel;
using Model;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ViewModel.Dialogs;



namespace ViewModel
{
    public class MainViewModel : ObservableObject
    {

        public ScreenStateLogger Capture { get; private set; }

        private readonly DialogService _dialogService = new DialogService();

        public MouseProjector MouseProjector { get; set; }

        /// <summary>
        /// X Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public int WindowXPos { get; set; } = 1888;

        /// <summary>
        /// Y Position of the upper left corner (Desktop in landsape mode).
        /// </summary>
        public int WindowYPos { get; set; } = 123;

        /// <summary>
        /// Width of the capture (Desktop in landsape mode).
        /// </summary>
        public int WindowCaptureWidth { get; set; } = 672;

        /// <summary>
        /// Height of the capture (Desktop in landsape mode).
        /// </summary>
        public int WindowCaptureHeight { get; set; } = 1194;

        public int PanelIconXPos { get; set; } = 2473;
        public int PanelIconYPos { get; set; } = 145;

        // end of configurable properties

        public WriteableBitmap FrameBuffer { get; }

        public MyScreenCapturer Capturer { get; set; }

        public bool IsInIdle { get; private set; } = true;

        public bool IsNotInIdle { get => !IsInIdle; }

        public MainViewModel()
        {
            TemplateMatcher.TemplateInImage("./Resources/screenshot1.png", "./Resources/panelIconTemplate.png");
            FrameBuffer = new WriteableBitmap(WindowCaptureWidth, WindowCaptureHeight, 96, 96, PixelFormats.Bgra32, null);
            Capture = new ScreenStateLogger();
            MouseProjector = new MouseProjector(WindowXPos, WindowYPos);
            Capturer = new MyScreenCapturer();
            Capturer.FrameCaptured += Capturer_FrameCaptured;
            Capturer.StartCapture();
        }

        private void Capturer_FrameCaptured(object? sender, FrameCapturedEventArgs args)
        {
            using var cropped = args.FrameBitmap.Clone(new Rectangle(WindowXPos, WindowYPos, WindowCaptureWidth, WindowCaptureHeight), args.FrameBitmap.PixelFormat);

            try
            {
                FrameBuffer.Dispatcher.Invoke(() =>
                {
                    FrameBuffer.Lock();
                    var data = cropped.LockBits(
                        new Rectangle(0, 0, WindowCaptureWidth, WindowCaptureHeight),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    FrameBuffer.WritePixels(
                      new Int32Rect(0, 0, WindowCaptureWidth, WindowCaptureHeight),
                      data.Scan0, data.Stride * WindowCaptureHeight, data.Stride);

                    cropped.UnlockBits(data);
                    FrameBuffer.AddDirtyRect(new Int32Rect(0, 0, WindowCaptureWidth, WindowCaptureHeight));
                    FrameBuffer.Unlock();
                }, DispatcherPriority.Normal, CancellationToken.None);
            }
            // raised when application gets closed
            catch (TaskCanceledException e)
            {

            }
        }

        private Bitmap TakeScreenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }

        public void ClosePanel()
        {
            IsInIdle = true;
            NativeMethods.SendClickWithoutMoving(PanelIconXPos, PanelIconYPos);
        }

        /// <summary>
        /// Handles a captured mousclick.
        /// </summary>
        /// <param name="relativeX"></param>
        /// <param name="relativeY"></param>
        public void HandleMouseClick(double relativeX, double relativeY)
        {
            if (IsInIdle)
            {
                if (TemplateMatcher.TemplateInImage(TakeScreenshot(), "./Resources/panelIconTemplate.png"))
                {
                    NativeMethods.SendClickWithoutMoving(PanelIconXPos, PanelIconYPos);
                    IsInIdle = false;
                    Thread.Sleep(150);
                    Capturer.StartCapture();
                }

            }
            else
                MouseProjector.HandleMouseClick((int)(WindowCaptureWidth * relativeX), (int)(WindowCaptureHeight * relativeY));
        }

        public void OnNewFrame(object? sender, Bitmap frame)
        {
            // 1. Crop to region of interest
            using var cropped = frame.Clone(new Rectangle(WindowXPos, WindowYPos, WindowCaptureWidth, WindowCaptureHeight), frame.PixelFormat);

            try
            {
                FrameBuffer.Dispatcher.Invoke(() =>
                {
                    FrameBuffer.Lock();
                    var data = cropped.LockBits(
                        new Rectangle(0, 0, WindowCaptureWidth, WindowCaptureHeight),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    FrameBuffer.WritePixels(
                      new Int32Rect(0, 0, WindowCaptureWidth, WindowCaptureHeight),
                      data.Scan0, data.Stride * WindowCaptureHeight, data.Stride);

                    cropped.UnlockBits(data);
                    FrameBuffer.AddDirtyRect(new Int32Rect(0, 0, WindowCaptureWidth, WindowCaptureHeight));
                    FrameBuffer.Unlock();
                }, DispatcherPriority.Normal, CancellationToken.None);
            } 
            // raised when application gets closed
            catch (TaskCanceledException e)
            {

            }

        }

        public Bitmap WriteableBitmapToBitmap(WriteableBitmap wbmp)
        {
            int w = wbmp.PixelWidth, h = wbmp.PixelHeight;
            var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(
                new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, bmp.PixelFormat);
            wbmp.CopyPixels(
                new Int32Rect(0, 0, w, h),
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
