namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Encapsulates the complete result of parsing and converting spawn data, including all validated spawns and issues.
/// </summary>
public sealed class ConversionResult
{
    private readonly List<SpawnArea> _spawns = new();
    private readonly List<ConversionIssue> _issues = new();

    public IReadOnlyList<SpawnArea> Spawns => _spawns;
    public IReadOnlyList<ConversionIssue> Issues => _issues;

    public int TotalLinesRead { get; set; }
    public int SuccessCount => _spawns.Count;
    public int WarningCount => _issues.Count(i => i.Severity == IssueSeverity.Warning);
    public int ErrorCount => _issues.Count(i => i.Severity == IssueSeverity.Error);
    public bool HasErrors => ErrorCount > 0;

    public void AddSpawn(SpawnArea spawn) => _spawns.Add(spawn);

    public void AddIssue(IssueSeverity severity, int lineNumber, string rawLine, string message)
    {
        _issues.Add(new ConversionIssue
        {
            Severity = severity,
            LineNumber = lineNumber,
            RawLine = rawLine,
            Message = message
        });
    }

    /// <summary>
    /// Gets all unique map numbers present in this conversion result.
    /// </summary>
    public IEnumerable<short> GetMapNumbers()
    {
        return _spawns.Select(s => s.MapNumber).Distinct().OrderBy(m => m);
    }

    /// <summary>
    /// Filters spawns belonging to a specific map.
    /// </summary>
    public IReadOnlyList<SpawnArea> GetSpawnsForMap(short mapNumber)
    {
        return _spawns.Where(s => s.MapNumber == mapNumber).ToList();
    }
}
