namespace Fingertippp.Core.Benchmarking;

public sealed record ClickTimingSample(DateTimeOffset Timestamp, bool IsPress, bool IsDoubleClickCandidate);