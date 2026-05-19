namespace Fingertippp.Core.Devices;

public sealed record MouseProfile(
    int VendorId,
    string Brand,
    string DisplayName,
    bool SupportsSoftwareAssistedOptimization,
    bool SupportsPollingOptimization,
    bool SupportsTrueDebounceControl);