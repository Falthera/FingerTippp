namespace Fingertippp.Core.Devices;

public sealed class MouseCapabilityClassifier
{
    private readonly MouseProfileDatabase profileDatabase;

    public MouseCapabilityClassifier(MouseProfileDatabase profileDatabase)
    {
        this.profileDatabase = profileDatabase;
    }

    public MouseCapabilityAssessment Assess(MouseDeviceFingerprint fingerprint)
    {
        if (!fingerprint.VendorId.HasValue)
        {
            return new MouseCapabilityAssessment(
                MouseCapabilityLevel.PollingOptimizationOnly,
                0.25,
                "Vendor identity was not exposed by Raw Input, so only transport-level optimization is selected.");
        }

        if (profileDatabase.TryGetProfile(fingerprint.VendorId.Value, out var profile))
        {
            if (profile.SupportsTrueDebounceControl)
            {
                return new MouseCapabilityAssessment(
                    MouseCapabilityLevel.TrueDebounceControl,
                    0.92,
                    $"{profile.Brand} profile is verified for firmware-level debounce control.");
            }

            if (profile.SupportsSoftwareAssistedOptimization)
            {
                return new MouseCapabilityAssessment(
                    MouseCapabilityLevel.SoftwareAssistedOptimization,
                    0.82,
                    $"{profile.Brand} profile supports software-assisted latency tuning.");
            }
        }

        return fingerprint.Transport switch
        {
            MouseTransportKind.Bluetooth => new MouseCapabilityAssessment(
                MouseCapabilityLevel.PollingOptimizationOnly,
                0.45,
                "Bluetooth mice can receive polling diagnostics and timing analysis, but firmware debounce control is not assumed."),
            MouseTransportKind.Wireless => new MouseCapabilityAssessment(
                MouseCapabilityLevel.SoftwareAssistedOptimization,
                0.55,
                "Wireless mice receive software-assisted optimization and polling stability analysis where possible."),
            MouseTransportKind.Usb => new MouseCapabilityAssessment(
                MouseCapabilityLevel.SoftwareAssistedOptimization,
                0.60,
                "USB mice without a specific profile receive software-assisted optimization and benchmark guidance."),
            _ => new MouseCapabilityAssessment(
                MouseCapabilityLevel.Unsupported,
                0.20,
                "The device could not be classified reliably enough for a safe optimization recommendation.")
        };
    }
}

public sealed record MouseDeviceFingerprint(int? VendorId, int? ProductId, MouseTransportKind Transport, string RawInputPath);

public sealed record MouseCapabilityAssessment(MouseCapabilityLevel Level, double Confidence, string Reason);