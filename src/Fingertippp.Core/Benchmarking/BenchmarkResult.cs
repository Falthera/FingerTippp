namespace Fingertippp.Core.Benchmarking;

public sealed record BenchmarkResult(
    int SampleCount,
    double AverageIntervalMilliseconds,
    double MedianIntervalMilliseconds,
    double JitterMilliseconds,
    double EstimatedCps,
    int DoubleClickCandidates);