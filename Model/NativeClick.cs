using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

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

        public static void SendClickWithoutMoving(int x, int y)
        {
            // 1. Find the window under that screen-coordinate
            var pt = new Point(x, y);
            IntPtr hWnd = NativeMethods.WindowFromPoint(pt);

            if (hWnd == IntPtr.Zero)
                return;  // no window there

            // 2. Convert to client-area coords
            NativeMethods.ScreenToClient(hWnd, ref pt);
            int lParam = (pt.Y << 16) | (pt.X & 0xFFFF);

            // 3. “Click” it
            NativeMethods.SendMessage(hWnd,
                NativeMethods.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(lParam));
            NativeMethods.SendMessage(hWnd,
                NativeMethods.WM_LBUTTONUP, IntPtr.Zero, new IntPtr(lParam));
        }
    }
}