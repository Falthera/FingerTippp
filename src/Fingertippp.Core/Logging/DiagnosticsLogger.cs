namespace Fingertippp.Core.Logging;

public sealed class DiagnosticsLogger
{
    private readonly List<string> entries = new();

    public void Info(string message)
    {
        entries.Add($"INFO {DateTimeOffset.UtcNow:O} {message}");
    }

    public void Warn(string message)
    {
        entries.Add($"WARN {DateTimeOffset.UtcNow:O} {message}");
    }

    public void Error(string message)
    {
        entries.Add($"ERROR {DateTimeOffset.UtcNow:O} {message}");
    }

    public IReadOnlyList<string> GetEntries() => entries;
}