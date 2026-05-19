using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Fingertippp.App.Infrastructure;

public sealed class MouseHotplugMonitor : IDisposable
{
    private const int WmInputDeviceChange = 0x00FE;
    private const uint RidDevNotify = 0x00002000;
    private readonly Action refreshCallback;
    private readonly HwndSource source;

    private MouseHotplugMonitor(HwndSource source, Action refreshCallback)
    {
        this.source = source;
        this.refreshCallback = refreshCallback;
        this.source.AddHook(WndProc);
    }

    public static MouseHotplugMonitor Attach(Window window, Action refreshCallback)
    {
        var helper = new WindowInteropHelper(window);
        var hwndSource = HwndSource.FromHwnd(helper.Handle) ?? throw new InvalidOperationException("The window handle is not available yet.");

        RegisterForDeviceNotifications(helper.Handle);
        return new MouseHotplugMonitor(hwndSource, refreshCallback);
    }

    public void Dispose()
    {
        source.RemoveHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmInputDeviceChange)
        {
            refreshCallback();
            handled = false;
        }

        return IntPtr.Zero;
    }

    private static void RegisterForDeviceNotifications(IntPtr hwnd)
    {
        var devices = new RawInputDeviceRegistration
        {
            UsagePage = 0x01,
            Usage = 0x02,
            Flags = RidDevNotify,
            TargetWindowHandle = hwnd
        };

        if (!RegisterRawInputDevices(new[] { devices }, 1, (uint)Marshal.SizeOf<RawInputDeviceRegistration>()))
        {
            throw new InvalidOperationException($"Raw Input device notification registration failed with error {Marshal.GetLastWin32Error()}.");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterRawInputDevices([In] RawInputDeviceRegistration[] pRawInputDevices, uint uiNumDevices, uint cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputDeviceRegistration
    {
        public ushort UsagePage;
        public ushort Usage;
        public uint Flags;
        public IntPtr TargetWindowHandle;
    }
}