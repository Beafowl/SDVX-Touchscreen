using CommunityToolkit.Mvvm.ComponentModel;
using Model;
using Model.Enums;
using Model.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ViewModel.Dialogs;



namespace ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly DialogService _dialogService = new DialogService();

        private readonly double _panelx1 = 0.9564054663326238;
        private readonly double _panely1 = 0.81556195965417866;
        private readonly double _panelx2 = 1;
        private readonly double _panely2 = 0.92122958693563883;

        public MouseProjector MouseProjector { get; set; }

        public AppConfiguration AppConfig { get; set; }

        public WriteableBitmap FrameBuffer { get; }

        public ScreenCapturer Capturer { get; set; }

        public bool IsInIdle { get; private set; } = true;

        public bool IsNotInIdle { get => !IsInIdle; }

        public MainViewModel()
        {
            AppConfig = ConfigurationFactory.GetConfiguration(true, Resolution._1440P);
            FrameBuffer = new WriteableBitmap(AppConfig.WindowCaptureHeight, AppConfig.WindowCaptureWidth, 96, 96, PixelFormats.Bgra32, null);
            MouseProjector = new MouseProjector(AppConfig.WindowXPos, AppConfig.WindowYPos);
            Capturer = new ScreenCapturer();
            Capturer.FrameCaptured += Capturer_FrameCaptured;
            Capturer.StartCapture();
        }

        private void Capturer_FrameCaptured(object? sender, FrameCapturedEventArgs args)
        {
            using var cropped = args.FrameBitmap.Clone(new Rectangle(AppConfig.WindowXPos, AppConfig.WindowYPos, AppConfig.WindowCaptureWidth, AppConfig.WindowCaptureHeight), args.FrameBitmap.PixelFormat);
            
            if (AppConfig.IsInLandscapeMode)
                cropped.RotateFlip(RotateFlipType.Rotate90FlipNone);

            try
            {
                FrameBuffer.Dispatcher.Invoke(() =>
                {
                    FrameBuffer.Lock();
                    var data = cropped.LockBits(
                        new Rectangle(0, 0, cropped.Width, cropped.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    FrameBuffer.WritePixels(
                      new Int32Rect(0, 0, cropped.Width, cropped.Height),
                      data.Scan0, data.Stride * cropped.Width, data.Stride);

                    cropped.UnlockBits(data);
                    FrameBuffer.AddDirtyRect(new Int32Rect(0, 0, AppConfig.WindowCaptureHeight, AppConfig.WindowCaptureWidth));

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
            NativeMethods.SendMouseEvent(AppConfig.PanelIconXPos, AppConfig.PanelIconYPos);
        }

        /// <summary>
        /// Handles a captured mousclick.
        /// </summary>
        /// <param name="relativeX"></param>
        /// <param name="relativeY"></param>
        public void HandleMouseClick(double relativeX, double relativeY, MouseEvent mouseEvent = MouseEvent.Click)
        {
            if (IsInIdle && mouseEvent == MouseEvent.Pressed)
            {
                // clickable panel icon found
                if (TemplateMatcher.TemplateInImage(TakeScreenshot(), "./Resources/panelIconTemplate.png"))
                {
                    NativeMethods.SendMouseEvent(AppConfig.PanelIconXPos, AppConfig.PanelIconYPos);
                    IsInIdle = false;
                    Thread.Sleep(150);
                    Capturer.StartCapture();
                }

            }
            else
            {
                // if the click happened in the area where the panel icon is, go to idle
                if (_panelx1 < relativeX && _panely1 < relativeY && mouseEvent == MouseEvent.Pressed)
                {
                    if (_panelx2 > relativeX && _panely2 > relativeY)
                    {
                        NativeMethods.SendMouseEvent(AppConfig.PanelIconXPos, AppConfig.PanelIconYPos);
                        IsInIdle = true;
                        return;
                    }
                }
                int posX = (int)(AppConfig.WindowCaptureHeight * (1 - relativeX));
                int posY = (int)(AppConfig.WindowCaptureWidth * relativeY);
                NativeMethods.SendMouseEvent(AppConfig.WindowXPos + posY, AppConfig.WindowYPos + posX, mouseEvent);
            }
        }

        public void GoToIdle()
        {
            if (IsInIdle)
                return;
            IsInIdle = true;
        }

        public void OnNewFrame(object? sender, Bitmap frame)
        {
            // 1. Crop to region of interest
            using var cropped = frame.Clone(new Rectangle(AppConfig.WindowXPos, AppConfig.WindowYPos, AppConfig.WindowCaptureWidth, AppConfig.WindowCaptureHeight), frame.PixelFormat);

            try
            {
                FrameBuffer.Dispatcher.Invoke(() =>
                {
                    FrameBuffer.Lock();
                    var data = cropped.LockBits(
                        new Rectangle(0, 0, AppConfig.WindowCaptureWidth, AppConfig.WindowCaptureHeight),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    FrameBuffer.WritePixels(
                      new Int32Rect(0, 0, AppConfig.WindowCaptureWidth, AppConfig.WindowCaptureHeight),
                      data.Scan0, data.Stride * AppConfig.WindowCaptureHeight, data.Stride);

                    cropped.UnlockBits(data);
                    FrameBuffer.AddDirtyRect(new Int32Rect(0, 0, AppConfig.WindowCaptureWidth, AppConfig.WindowCaptureHeight));
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
