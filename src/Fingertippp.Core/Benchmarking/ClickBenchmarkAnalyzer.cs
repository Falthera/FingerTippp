namespace Fingertippp.Core.Benchmarking;

public sealed class ClickBenchmarkAnalyzer
{
    public BenchmarkResult Analyze(IReadOnlyList<ClickTimingSample> samples)
    {
        if (samples.Count < 2)
        {
            return new BenchmarkResult(0, 0, 0, 0, 0, 0);
        }

        var intervals = new List<double>(samples.Count - 1);
        var doubleClickCandidates = 0;

        for (var index = 1; index < samples.Count; index++)
        {
            var previous = samples[index - 1].Timestamp;
            var current = samples[index].Timestamp;
            var interval = (current - previous).TotalMilliseconds;
            intervals.Add(interval);

            if (samples[index].IsDoubleClickCandidate)
            {
                doubleClickCandidates++;
            }
        }

        intervals.Sort();
        var average = intervals.Average();
        var median = intervals.Count % 2 == 0
            ? (intervals[intervals.Count / 2 - 1] + intervals[intervals.Count / 2]) / 2.0
            : intervals[intervals.Count / 2];
        var jitter = Math.Sqrt(intervals.Select(interval => Math.Pow(interval - average, 2)).Average());
        var cps = average > 0 ? 1000.0 / average : 0;

        return new BenchmarkResult(samples.Count, average, median, jitter, cps, doubleClickCandidates);
    }
}