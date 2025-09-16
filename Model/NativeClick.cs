using System.Runtime.InteropServices;
using Model.Enums;

namespace Model
{
    public static class NativeMethods
    {
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point p);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point p);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static void SendMouseEvent(int x, int y, MouseEvent clickType = MouseEvent.Click)
        {
            // 1. Find the window under that screen-coordinate
            var pt = new Point(x, y);
            IntPtr hWnd = WindowFromPoint(pt);

            if (hWnd == IntPtr.Zero)
                return;  // no window there

            // 2. Convert to client-area coords
            ScreenToClient(hWnd, ref pt);
            int lParam = (pt.Y << 16) | (pt.X & 0xFFFF);

            if (clickType == MouseEvent.Click)
            {
                SendMessage(hWnd, WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(lParam));
                SendMessage(hWnd, WM_LBUTTONUP, IntPtr.Zero, new IntPtr(lParam));
            }
            else if (clickType == MouseEvent.Pressed)
                SendMessage(hWnd, WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(lParam));
            else
                SendMessage(hWnd, WM_LBUTTONUP, IntPtr.Zero, new IntPtr(lParam));
        }
    }
}