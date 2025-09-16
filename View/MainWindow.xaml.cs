using System.Windows;
using System.Windows.Input;
using ViewModel;
using Model.Enums;
using System.Printing;
using System.Windows.Controls;

namespace View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _fullscreenActive = false;
        private bool _leftClickPressed = false;

        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = (MainViewModel)DataContext;
        }

        private void OnMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border) return;
            _leftClickPressed = true;
            Point p = e.GetPosition(Buffer);

            int x = (int)p.X;
            int y = (int)p.Y;

            _vm.HandleMouseClick(x / Buffer.ActualWidth, y / Buffer.ActualHeight, MouseEvent.Pressed);
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            _leftClickPressed = false;
            Point p = e.GetPosition(Buffer);

            int x = (int)p.X;
            int y = (int)p.Y;

            _vm.HandleMouseClick(x / Buffer.ActualWidth, y / Buffer.ActualHeight, MouseEvent.Released);
            e.Handled = true;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_leftClickPressed)
                return;
            Point p = e.GetPosition(Buffer);

            int x = (int)p.X;
            int y = (int)p.Y;

            _vm.HandleMouseClick(x / Buffer.ActualWidth, y / Buffer.ActualHeight, MouseEvent.Pressed);
            e.Handled = true;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
                ToggleFullscreen();
            else if (e.Key == Key.Escape)
                _vm.ClosePanel();
            else if (e.Key == Key.Q)
                Environment.Exit(0);
            e.Handled = true;
        }

        private void OnClosePanel(object? sender, MouseEventArgs e)
        {
            _vm.GoToIdle();
            e.Handled = true;
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

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                // sometimes this returns an exception when debugging
                myMediaElement.Position = new TimeSpan(0, 0, 1);
                myMediaElement.Play();
            }
            catch {}
        }
    }
}