using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using ScreenCapturerNS;

namespace Model
{
    public class ScreenStateLogger : INotifyPropertyChanged
    {
        public bool IsRunning { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public Image FrameBuffer { get; private set; }

        public int MaximumFramerate { get; set; } = 60;

        private bool _init;


        public void Start()
        {
            Task.Run(CaptureLoop);
        }

        private void CaptureLoop()
        {
            _init = true;
            bool _screenCapInit = true;
            while (_init)
            {

                try
                {
                    ScreenCapturer.StartCapture((Bitmap bitmap) =>
                    {
                        FrameCreated?.Invoke(this, bitmap);
                    });
                }
                // when exclusive fullscreen gets initialized or deinitialized,
                // an exceptions is thrown. simply restart the capturer
                catch (Exception ex)
                {
                    ScreenCapturer.StopCapture();
                }
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<Bitmap> FrameCreated;
    }
}
