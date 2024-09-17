using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        protected enum MonitorDpiType
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        public Point CurrentDpi { get; private set; }

        public bool IsPerMonitorEnabled;

        public Point ScaleFactor { get; private set; }

        protected HwndSource source;

        protected Point systemDpi;

        protected Point WpfDpi { get; set; }


        public MainWindow()
        {
            InitializeComponent();

            // Watch for SystemEvent notifications
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            // Set up the SourceInitialized event handler
            SourceInitialized += DpiAwareWindow_SourceInitialized;
        }

        ~MainWindow()
        {
            // Deregister our SystemEvents handler
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private void DpiAwareWindow_SourceInitialized(object sender, EventArgs e)
        {
            source = (HwndSource)HwndSource.FromVisual(this);
            source.AddHook(WindowProcedureHook);

            // Determine if this application is Per Monitor DPI Aware.
            IsPerMonitorEnabled = GetPerMonitorDPIAware() == ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware;

            // Is the window in per-monitor DPI mode?
            if (IsPerMonitorEnabled)
            {
                // It is.  Calculate the DPI used by the System.
                systemDpi = GetSystemDPI();

                // Calculate the DPI used by WPF.
                WpfDpi = new Point
                {
                    X = 96.0 * source.CompositionTarget.TransformToDevice.M11,
                    Y = 96.0 * source.CompositionTarget.TransformToDevice.M22
                };

                // Get the Current DPI of the monitor of the window.
                CurrentDpi = GetDpiForHwnd(source.Handle);

                // Calculate the scale factor used to modify window size, graphics and text.
                ScaleFactor = new Point
                {
                    X = CurrentDpi.X / WpfDpi.X,
                    Y = CurrentDpi.Y / WpfDpi.Y
                };

                // Update Width and Height based on the on the current DPI of the monitor
                Width = Width * ScaleFactor.X;
                Height = Height * ScaleFactor.Y;

                // Update graphics and text based on the current DPI of the monitor.
                UpdateLayoutTransform(ScaleFactor);
            }
        }

        protected Point GetDpiForHwnd(IntPtr hwnd)
        {
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            uint newDpiX = 96;
            uint newDpiY = 96;
            if (GetDpiForMonitor(monitor, (int)MonitorDpiType.MDT_Effective_DPI, ref newDpiX, ref newDpiY) != 0)
            {
                return new Point
                {
                    X = 96.0,
                    Y = 96.0
                };
            }

            return new Point
            {
                X = (double)newDpiX,
                Y = (double)newDpiY
            };
        }
        public static ProcessDpiAwareness GetPerMonitorDPIAware()
        {
            ProcessDpiAwareness awareness = ProcessDpiAwareness.Process_DPI_Unaware;

            try
            {
                Process curProcess = Process.GetCurrentProcess();
                int result = GetProcessDpiAwareness(curProcess.Handle, ref awareness);
                if (result != 0)
                {
                    throw new Exception("Unable to read process DPI level");
                }

            }
            catch (DllNotFoundException)
            {
                try
                {
                    // We're running on either Vista, Windows 7 or Windows 8.  Return the correct ProcessDpiAwareness value.
                    awareness = IsProcessDpiAware() ? ProcessDpiAwareness.Process_System_DPI_Aware : ProcessDpiAwareness.Process_DPI_Unaware;

                }
                catch (EntryPointNotFoundException) { }

            }
            catch (EntryPointNotFoundException)
            {
                try
                {
                    // We're running on either Vista, Windows 7 or Windows 8.  Return the correct ProcessDpiAwareness value.
                    awareness = IsProcessDpiAware() ? ProcessDpiAwareness.Process_System_DPI_Aware : ProcessDpiAwareness.Process_DPI_Unaware;

                }
                catch (EntryPointNotFoundException) { }
            }

            // Return the value in awareness.
            return awareness;
        }

        public static Point GetSystemDPI()
        {
            IntPtr hDC = GetDC(IntPtr.Zero);
            int newDpiX = GetDeviceCaps(hDC, LOGPIXELSX);
            int newDpiY = GetDeviceCaps(hDC, LOGPIXELSY);
            ReleaseDC(IntPtr.Zero, hDC);

            return new Point
            {
                X = (double)newDpiX,
                Y = (double)newDpiY
            };
        }

        public void OnDPIChanged()
        {
            ScaleFactor = new Point
            {
                X = CurrentDpi.X / WpfDpi.X,
                Y = CurrentDpi.Y / WpfDpi.Y
            };

            UpdateLayoutTransform(ScaleFactor);
        }

        public virtual void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            // Get the handle for this window.  Need to worry about a window that has been created by not yet displayed.
            IntPtr handle = source == null ? new HwndSource(new HwndSourceParameters()).Handle : source.Handle;

            // Get the current DPI for the window we're on.
            CurrentDpi = GetDpiForHwnd(handle);

            // Adjust the scale factor.
            ScaleFactor = new Point
            {
                X = CurrentDpi.X / WpfDpi.X,
                Y = CurrentDpi.Y / WpfDpi.Y
            };

            // Update the layout transform
            UpdateLayoutTransform(ScaleFactor);
        }

        private void UpdateLayoutTransform(Point scaleFactor)
        {
            if (IsPerMonitorEnabled)
            {
                if (ScaleFactor.X != 1.0 || ScaleFactor.Y != 1.0)
                {
                    LayoutTransform = new ScaleTransform(scaleFactor.X, scaleFactor.Y);
                }
                else
                {
                    LayoutTransform = null;
                }
            }
        }

        public virtual IntPtr WindowProcedureHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Determine which Monitor is displaying the Window
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            // Switch on the message.
            switch ((WinMessages)msg)
            {
                case WinMessages.WM_DPICHANGED:
                    // Marshal the value in the lParam into a Rect.
                    RECT newDisplayRect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

                    // Set the Window's position & size.
                    Vector ul = source.CompositionTarget.TransformFromDevice.Transform(new Vector(newDisplayRect.left, newDisplayRect.top));
                    Vector hw = source.CompositionTarget.TransformFromDevice.Transform(new Vector(newDisplayRect.right = newDisplayRect.left, newDisplayRect.bottom - newDisplayRect.top));
                    Left = ul.X;
                    Top = ul.Y;
                    Width = hw.X;
                    Height = hw.Y;

                    // Remember the current DPI settings.
                    Point oldDpi = CurrentDpi;

                    // Get the new DPI settings from wParam
                    CurrentDpi = new Point
                    {
                        X = (double)(wParam.ToInt32() >> 16),
                        Y = (double)(wParam.ToInt32() & 0x0000FFFF)
                    };

                    if (oldDpi.X != CurrentDpi.X || oldDpi.Y != CurrentDpi.Y)
                    {
                        OnDPIChanged();
                    }

                    handled = true;
                    return IntPtr.Zero;

                case WinMessages.WM_GETMINMAXINFO:
                    // lParam has a pointer to the MINMAXINFO structure.  Marshal it into managed memory.
                    MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                    if (monitor != IntPtr.Zero)
                    {
                        MONITORINFO monitorInfo = new MONITORINFO();
                        GetMonitorInfo(monitor, monitorInfo);

                        // Get the Monitor's working area
                        RECT rcWorkArea = monitorInfo.rcWork;
                        RECT rcMonitorArea = monitorInfo.rcMonitor;

                        // Adjust the maximized size and position to fit the work area of the current monitor
                        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                    }

                    // Copy our changes to the mmi object back to the original
                    Marshal.StructureToPtr(mmi, lParam, true);
                    handled = false; // This line used to set handled to true
                    return IntPtr.Zero;

                default:
                    // Let the WPF code handle all other messages. Return 0.
                    return IntPtr.Zero;
            }
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        [DllImport("shcore.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern int GetDpiForMonitor(IntPtr hMonitor, int dpiType, ref uint xDpi, ref uint yDpi);

        [DllImport("user32")]
        protected static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("shcore.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern int GetProcessDpiAwareness(IntPtr handle, ref ProcessDpiAwareness awareness);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern bool IsProcessDpiAware();

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flag);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern void ReleaseDC(IntPtr hWnd, IntPtr hDC);
    }

    public enum SizeMessages
    {
        SIZE_RESTORED = 0,
        SIZE_MINIMIZED = 1,
        SIZE_MAXIMIZED = 2,
        SIZE_MAXSHOW = 3,
        SIZE_MAXHIDE = 4
    }

    public enum WinMessages : int
    {
        WM_DPICHANGED = 0x02E0,
        WM_GETMINMAXINFO = 0x0024,
        WM_SIZE = 0x0005,
        WM_WINDOWPOSCHANGING = 0x0046,
        WM_WINDOWPOSCHANGED = 0x0047,
    }

    public enum ProcessDpiAwareness
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct POINT
{
    public int x;

    public int y;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct MINMAXINFO
{
    public POINT ptReserved;

    public POINT ptMaxSize;

    public POINT ptMaxPosition;

    public POINT ptMinTrackSize;

    public POINT ptMaxTrackSize;
}

[Flags]
public enum MONITORINFO_Flags
{
    None = 0,
    MONITORINFOF_PRIMARY = 1
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct MONITORINFO
{
    public int cbSize;

    public RECT rcMonitor;

    public RECT rcWork;

    public MONITORINFO_Flags dwFlags;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
}