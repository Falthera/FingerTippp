using Fingertippp.Core.Devices;

namespace Fingertippp.Core.Optimization;

public sealed class OptimizationEngine
{
    public OptimizationRecommendation Recommend(MouseDeviceInfo device)
    {
        return device.CapabilityLevel switch
        {
            MouseCapabilityLevel.TrueDebounceControl => new OptimizationRecommendation(
                OptimizationStrategy.TrueDebounceControl,
                "true debounce control",
                "A verified vendor protocol is available for firmware-level debounce configuration.",
                device.Confidence,
                device.CapabilityLevel,
                true,
                new[] { "Show vendor-safe debounce controls", "Validate current firmware profile", "Prompt before applying changes" }),
            MouseCapabilityLevel.SoftwareAssistedOptimization => new OptimizationRecommendation(
                OptimizationStrategy.SoftwareAssistedOptimization,
                "software-assisted optimization",
                "The device does not expose verified firmware debounce control, so safe Windows-side responsiveness tuning is selected.",
                device.Confidence,
                device.CapabilityLevel,
                true,
                new[] { "Enable Raw Input prioritization", "Measure click timing", "Offer safe latency profile" }),
            MouseCapabilityLevel.PollingOptimizationOnly => new OptimizationRecommendation(
                OptimizationStrategy.PollingOptimizationOnly,
                "polling optimization only",
                "The device should only receive transport and timing diagnostics because no verified device-specific tuning was found.",
                device.Confidence,
                device.CapabilityLevel,
                false,
                new[] { "Monitor polling stability", "Collect jitter diagnostics", "Export baseline benchmark" }),
            _ => new OptimizationRecommendation(
                OptimizationStrategy.Unsupported,
                "unsupported",
                "This device cannot be optimized safely beyond passive diagnostics.",
                device.Confidence,
                device.CapabilityLevel,
                false,
                Array.Empty<string>())
        };
    }
}