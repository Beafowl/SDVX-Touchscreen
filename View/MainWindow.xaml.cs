using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModel;

namespace View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _fullscreenActive = false;

        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = (MainViewModel)DataContext;

        }

        private void OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Point p = e.GetPosition(Buffer);

            int x = (int)p.X;
            int y = (int)p.Y;

            _vm.HandleMouseClick(x / Buffer.ActualWidth, y / Buffer.ActualHeight);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F11)
                ToggleFullscreen();
            else if (e.Key == Key.Escape)
                _vm.ClosePanel();
            else if (e.Key == Key.Q)
                Environment.Exit(0);
        }

        private void ToggleFullscreen()
        {
            if (_fullscreenActive)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            _fullscreenActive = !_fullscreenActive;
        }

        private void mediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            myMediaElement.Position = new TimeSpan(0, 0, 1);
            myMediaElement.Play();
        }
    }
}