using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SystemMonitor
{
    public static class Helper
    {
        const uint SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_NOACTIVATE = 0x0010;
        const int WS_EX_TOOLWINDOW = 0x00000080, GWL_EXSTYLE = -20;

        static readonly string[] sizes = { "B", "KB", "MB", "GB", "TB" };

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        /// <summary>
        /// Set the window to "always bottom" and hide from task switcher and task bar
        /// </summary>
        /// <param name="window">Window to set properties</param>
        public static void SetBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            int exStyle = (int)GetWindowLong(hWnd, GWL_EXSTYLE);

            exStyle |= WS_EX_TOOLWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, (IntPtr)exStyle);
        }

        /// <summary>
        /// Get bytes converted to readable string
        /// </summary>
        /// <param name="bytes">Number of bytes</param>
        /// <returns>A string like 3 MB</returns>
        public static string GetBytesFormatted(ulong bytes)
        {
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return string.Format("{0:0.0} {1}", len, sizes[order]);
        }

        /// <summary>
        /// Get bytes converted to readable string
        /// </summary>
        /// <param name="bytes">Number of bytes</param>
        /// <returns>A string like 3 MB</returns>
        public static string GetBytesFormatted(long bytes)
        {
            return GetBytesFormatted((ulong)bytes);
        }

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }
    }
}
