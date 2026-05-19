using Fingertippp.Core.Devices;

namespace Fingertippp.Core.Optimization;

public sealed record OptimizationRecommendation(
    OptimizationStrategy Strategy,
    string DisplayLabel,
    string Explanation,
    double Confidence,
    MouseCapabilityLevel CapabilityLevel,
    bool RequiresConfirmation,
    IReadOnlyList<string> Actions);