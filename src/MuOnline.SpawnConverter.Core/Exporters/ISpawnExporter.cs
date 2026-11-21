namespace MuOnline.SpawnConverter.Core.Exporters;

using MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Abstraction for exporting converted spawn models to target formats.
/// </summary>
public interface ISpawnExporter
{
    /// <summary>
    /// Gets the unique format identifier (e.g., "openmu", "json", "msb").
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Exports spawns to a serialized string.
    /// </summary>
    string Export(IEnumerable<SpawnArea> spawns);
}
