namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Severity level for conversion issues.
/// </summary>
public enum IssueSeverity
{
    Information,
    Warning,
    Error
}

/// <summary>
/// Represents a non-silent warning, error or informational notice encountered during file parsing or conversion.
/// </summary>
public sealed record ConversionIssue
{
    public IssueSeverity Severity { get; init; } = IssueSeverity.Warning;
    public int LineNumber { get; init; }
    public string RawLine { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"[{Severity.ToString().ToUpperInvariant()}] Line {LineNumber}: {Message} (Raw: \"{RawLine}\")";
    }
}
