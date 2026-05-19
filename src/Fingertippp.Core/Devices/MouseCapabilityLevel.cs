namespace Fingertippp.Core.Devices;

public enum MouseCapabilityLevel
{
    Unsupported = 0,
    PollingOptimizationOnly = 1,
    SoftwareAssistedOptimization = 2,
    TrueDebounceControl = 3
}