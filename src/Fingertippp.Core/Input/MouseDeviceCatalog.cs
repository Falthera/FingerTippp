using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Fingertippp.Core.Devices;
using Fingertippp.Core.Optimization;

namespace Fingertippp.Core.Input;

public sealed class MouseDeviceCatalog
{
    private static readonly Regex VendorProductPattern = new(@"VID_([0-9A-F]{4}).*PID_([0-9A-F]{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly MouseCapabilityClassifier capabilityClassifier;
    private readonly OptimizationEngine optimizationEngine;

    public MouseDeviceCatalog(MouseCapabilityClassifier capabilityClassifier, OptimizationEngine optimizationEngine)
    {
        this.capabilityClassifier = capabilityClassifier;
        this.optimizationEngine = optimizationEngine;
    }

    public IReadOnlyList<MouseDeviceInfo> GetDevices()
    {
        var deviceFingerprints = new List<MouseDeviceFingerprint>();
        var count = 0u;

        if (NativeMethods.GetRawInputDeviceList(null, ref count, (uint)Marshal.SizeOf<RawInputDeviceList>()) != 0 || count == 0)
        {
            return Array.Empty<MouseDeviceInfo>();
        }

        var devices = new RawInputDeviceList[count];
        if (NativeMethods.GetRawInputDeviceList(devices, ref count, (uint)Marshal.SizeOf<RawInputDeviceList>()) == 0)
        {
            return Array.Empty<MouseDeviceInfo>();
        }

        foreach (var rawDevice in devices)
        {
            if (rawDevice.DeviceType != RawInputDeviceType.Mouse && rawDevice.DeviceType != RawInputDeviceType.Hid)
            {
                continue;
            }

            var name = QueryDeviceName(rawDevice.DeviceHandle);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            deviceFingerprints.Add(new MouseDeviceFingerprint(
                ExtractVendorId(name),
                ExtractProductId(name),
                DetectTransport(name),
                name));
        }

        var deduplicated = new Dictionary<string, MouseDeviceInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var fingerprint in deviceFingerprints)
        {
            var assessment = capabilityClassifier.Assess(fingerprint);
            var profile = optimizationEngine.Recommend(new MouseDeviceInfo(
                fingerprint.RawInputPath,
                BuildFriendlyName(fingerprint),
                fingerprint.VendorId,
                fingerprint.ProductId,
                fingerprint.Transport,
                assessment.Level,
                assessment.Confidence,
                assessment.Reason,
                true,
                fingerprint.RawInputPath,
                DateTimeOffset.UtcNow));

            var device = new MouseDeviceInfo(
                fingerprint.RawInputPath,
                BuildFriendlyName(fingerprint),
                fingerprint.VendorId,
                fingerprint.ProductId,
                fingerprint.Transport,
                assessment.Level,
                profile.Confidence,
                assessment.Reason,
                true,
                fingerprint.RawInputPath,
                DateTimeOffset.UtcNow);

            deduplicated[device.DeviceId] = device;
        }

        return deduplicated.Values.OrderBy(device => device.FriendlyName).ToArray();
    }

    private static string BuildFriendlyName(MouseDeviceFingerprint fingerprint)
    {
        if (fingerprint.VendorId is null)
        {
            return fingerprint.Transport switch
            {
                MouseTransportKind.Bluetooth => "Bluetooth mouse",
                MouseTransportKind.Wireless => "Wireless mouse",
                MouseTransportKind.Usb => "USB mouse",
                _ => "Unidentified mouse"
            };
        }

        return $"Mouse VID {fingerprint.VendorId:X4} PID {fingerprint.ProductId:X4}";
    }

    private static int? ExtractVendorId(string rawPath)
    {
        var match = VendorProductPattern.Match(rawPath);
        return match.Success ? Convert.ToInt32(match.Groups[1].Value, 16) : null;
    }

    private static int? ExtractProductId(string rawPath)
    {
        var match = VendorProductPattern.Match(rawPath);
        return match.Success ? Convert.ToInt32(match.Groups[2].Value, 16) : null;
    }

    private static MouseTransportKind DetectTransport(string rawPath)
    {
        if (rawPath.Contains("BTH", StringComparison.OrdinalIgnoreCase) || rawPath.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase))
        {
            return MouseTransportKind.Bluetooth;
        }

        if (rawPath.Contains("VID_", StringComparison.OrdinalIgnoreCase))
        {
            return MouseTransportKind.Usb;
        }

        return MouseTransportKind.Unknown;
    }

    private static string? QueryDeviceName(IntPtr deviceHandle)
    {
        var size = 0u;
        NativeMethods.GetRawInputDeviceInfo(deviceHandle, RawInputDeviceInfoCommand.DeviceName, null, ref size);
        if (size == 0)
        {
            return null;
        }

        var builder = new StringBuilder((int)size);
        return NativeMethods.GetRawInputDeviceInfo(deviceHandle, RawInputDeviceInfoCommand.DeviceName, builder, ref size) > 0
            ? builder.ToString()
            : null;
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputDeviceList([Out] RawInputDeviceList[]? pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetRawInputDeviceInfo(IntPtr hDevice, RawInputDeviceInfoCommand uiCommand, StringBuilder? pData, ref uint pcbSize);
    }

    private enum RawInputDeviceInfoCommand : uint
    {
        DeviceName = 0x20000007
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputDeviceList
    {
        public IntPtr DeviceHandle;
        public RawInputDeviceType DeviceType;
    }

    private enum RawInputDeviceType : uint
    {
        Mouse = 0,
        Keyboard = 1,
        Hid = 2
    }
}