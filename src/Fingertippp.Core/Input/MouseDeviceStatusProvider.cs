using Fingertippp.Core.Devices;
using Fingertippp.Core.Diagnostics;
using Fingertippp.Core.Optimization;

namespace Fingertippp.Core.Input;

public sealed class MouseDeviceStatusProvider
{
    private readonly MouseDeviceCatalog catalog;
    private readonly OptimizationEngine optimizationEngine;

    public MouseDeviceStatusProvider(MouseDeviceCatalog catalog, OptimizationEngine optimizationEngine)
    {
        this.catalog = catalog;
        this.optimizationEngine = optimizationEngine;
    }

    public IReadOnlyList<MouseDeviceInfo> GetDevices()
    {
        var devices = catalog.GetDevices();
        if (devices.Count > 0)
        {
            return devices;
        }

        var fallback = new MouseDeviceInfo(
            "fallback-generic",
            "Generic HID mouse",
            null,
            null,
            MouseTransportKind.Unknown,
            MouseCapabilityLevel.PollingOptimizationOnly,
            0.20,
            "No raw-input mouse devices were enumerated, so the app is using a passive fallback state.",
            true,
            string.Empty,
            DateTimeOffset.UtcNow);

        return new[] { fallback };
    }

    public DeviceStatus BuildStatus(MouseDeviceInfo device)
    {
        return new DeviceStatus(device, optimizationEngine.Recommend(device));
    }
}