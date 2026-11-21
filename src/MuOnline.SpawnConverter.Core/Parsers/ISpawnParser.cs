namespace MuOnline.SpawnConverter.Core.Parsers;

using MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Abstraction for parsing monster/NPC spawn configurations from various source formats.
/// </summary>
public interface ISpawnParser
{
    /// <summary>
    /// Parses spawns from text content.
    /// </summary>
    ConversionResult Parse(string content);

    /// <summary>
    /// Parses spawns from a file path.
    /// </summary>
    ConversionResult ParseFile(string filePath);
}
