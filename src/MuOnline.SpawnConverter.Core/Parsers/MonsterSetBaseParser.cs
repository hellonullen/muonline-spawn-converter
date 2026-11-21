namespace MuOnline.SpawnConverter.Core.Parsers;

using System.Globalization;
using System.Text.RegularExpressions;
using MuOnline.SpawnConverter.Core.Data;
using MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Robust parser for classic Webzen, MuEmu, and SCF format MonsterSetBase.txt files.
/// </summary>
public sealed class MonsterSetBaseParser : ISpawnParser
{
    private static readonly Regex TokenSplitRegex = new(@"\s+", RegexOptions.Compiled);

    public ConversionResult ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var missingResult = new ConversionResult();
            missingResult.AddIssue(IssueSeverity.Error, 0, filePath, $"File does not exist: {filePath}");
            return missingResult;
        }

        var content = File.ReadAllText(filePath);
        return Parse(content);
    }

    public ConversionResult Parse(string content)
    {
        var result = new ConversionResult();
        if (string.IsNullOrWhiteSpace(content))
        {
            result.AddIssue(IssueSeverity.Warning, 0, string.Empty, "Input content is empty.");
            return result;
        }

        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        result.TotalLinesRead = lines.Length;

        int currentSection = -1; // -1 indicates before any section or outside

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string rawLine = lines[i];
            string trimmed = rawLine.Trim();

            // Skip empty lines
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            // Extract inline comments
            string codePart = trimmed;
            string? comment = null;
            int commentIndex = trimmed.IndexOf("//", StringComparison.Ordinal);
            if (commentIndex >= 0)
            {
                codePart = trimmed[..commentIndex].Trim();
                comment = trimmed[(commentIndex + 2)..].Trim();
            }

            // If whole line was a comment
            if (string.IsNullOrEmpty(codePart))
            {
                continue;
            }

            // Check for section end
            if (string.Equals(codePart, "end", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = -1;
                continue;
            }

            // Check for section switch (0, 1, 2, 3, etc.)
            if (int.TryParse(codePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int newSection))
            {
                currentSection = newSection;
                continue;
            }

            // We are inside a section, parse tokens
            string[] tokens = TokenSplitRegex.Split(codePart)
                                             .Where(t => !string.IsNullOrEmpty(t))
                                             .ToArray();

            if (tokens.Length == 0)
            {
                continue;
            }

            ParseLine(result, currentSection, tokens, lineNumber, rawLine, comment);
        }

        return result;
    }

    private void ParseLine(
        ConversionResult result,
        int section,
        string[] tokens,
        int lineNumber,
        string rawLine,
        string? comment)
    {
        switch (section)
        {
            case 0:
                ParseSection0(result, tokens, lineNumber, rawLine, comment);
                break;
            case 1:
                ParseSection1(result, tokens, lineNumber, rawLine, comment);
                break;
            case 2:
                ParseSection2(result, tokens, lineNumber, rawLine, comment);
                break;
            default:
                // Section 3 or unspecified section - try auto-detect by token count
                if (tokens.Length >= 9)
                {
                    ParseSection1(result, tokens, lineNumber, rawLine, comment);
                }
                else if (tokens.Length >= 5)
                {
                    ParseSection0(result, tokens, lineNumber, rawLine, comment);
                }
                else
                {
                    result.AddIssue(
                        IssueSeverity.Warning,
                        lineNumber,
                        rawLine,
                        $"Unsupported or unrecognized section {section} with {tokens.Length} parameters; line skipped.");
                }
                break;
        }
    }

    /// <summary>
    /// Section 0: Static NPCs or Bosses.
    /// Columns: MobID MapID Distance X Y [Dir]
    /// </summary>
    private void ParseSection0(ConversionResult result, string[] tokens, int lineNumber, string rawLine, string? comment)
    {
        if (tokens.Length < 5)
        {
            result.AddIssue(IssueSeverity.Error, lineNumber, rawLine,
                $"Section 0 line requires at least 5 columns (MobID, MapID, Distance, X, Y), found {tokens.Length}.");
            return;
        }

        if (!TryParseInt(tokens[0], out int mobId) ||
            !TryParseShort(tokens[1], out short mapId) ||
            !TryParseInt(tokens[2], out int distance) ||
            !TryParseInt(tokens[3], out int x) ||
            !TryParseInt(tokens[4], out int y))
        {
            result.AddIssue(IssueSeverity.Error, lineNumber, rawLine, "Section 0 contains non-numeric column data.");
            return;
        }

        int webzenDir = -1;
        if (tokens.Length >= 6 && TryParseInt(tokens[5], out int parsedDir))
        {
            webzenDir = parsedDir;
        }

        if (!ValidateCoordinates(result, lineNumber, rawLine, x, y, x, y, out byte bX1, out byte bY1, out byte bX2, out byte bY2))
        {
            return;
        }

        ValidateMetadata(result, lineNumber, rawLine, mobId, mapId);

        var spawn = new SpawnArea
        {
            Id = Guid.NewGuid(),
            MonsterNumber = mobId,
            MapNumber = mapId,
            Distance = distance,
            X1 = bX1,
            Y1 = bY1,
            X2 = bX2,
            Y2 = bY2,
            Direction = DirectionHelper.FromWebzenDirection(webzenDir),
            Quantity = 1,
            SpawnTrigger = SpawnTrigger.Automatic,
            WaveNumber = 0,
            SpawnType = SpawnType.SingleNpcOrMonster,
            SourceLineNumber = lineNumber,
            Comment = comment
        };

        result.AddSpawn(spawn);
    }

    /// <summary>
    /// Section 1: Roaming/Area Monster Spawns.
    /// Columns: MobID MapID Distance X1 Y1 X2 Y2 [Dir] [Quantity]
    /// </summary>
    private void ParseSection1(ConversionResult result, string[] tokens, int lineNumber, string rawLine, string? comment)
    {
        if (tokens.Length < 7)
        {
            result.AddIssue(IssueSeverity.Error, lineNumber, rawLine,
                $"Section 1 line requires at least 7 columns (MobID, MapID, Distance, X1, Y1, X2, Y2), found {tokens.Length}.");
            return;
        }

        if (!TryParseInt(tokens[0], out int mobId) ||
            !TryParseShort(tokens[1], out short mapId) ||
            !TryParseInt(tokens[2], out int distance) ||
            !TryParseInt(tokens[3], out int x1) ||
            !TryParseInt(tokens[4], out int y1) ||
            !TryParseInt(tokens[5], out int x2) ||
            !TryParseInt(tokens[6], out int y2))
        {
            result.AddIssue(IssueSeverity.Error, lineNumber, rawLine, "Section 1 contains non-numeric column data.");
            return;
        }

        int webzenDir = -1;
        if (tokens.Length >= 8 && TryParseInt(tokens[7], out int parsedDir))
        {
            webzenDir = parsedDir;
        }

        short quantity = 1;
        if (tokens.Length >= 9 && TryParseShort(tokens[8], out short parsedQty))
        {
            quantity = parsedQty <= 0 ? (short)1 : parsedQty;
        }

        if (!ValidateCoordinates(result, lineNumber, rawLine, x1, y1, x2, y2, out byte bX1, out byte bY1, out byte bX2, out byte bY2))
        {
            return;
        }

        ValidateMetadata(result, lineNumber, rawLine, mobId, mapId);

        var spawn = new SpawnArea
        {
            Id = Guid.NewGuid(),
            MonsterNumber = mobId,
            MapNumber = mapId,
            Distance = distance,
            X1 = bX1,
            Y1 = bY1,
            X2 = bX2,
            Y2 = bY2,
            Direction = DirectionHelper.FromWebzenDirection(webzenDir),
            Quantity = quantity,
            SpawnTrigger = SpawnTrigger.Automatic,
            WaveNumber = 0,
            SpawnType = SpawnType.AreaSpawn,
            SourceLineNumber = lineNumber,
            Comment = comment
        };

        result.AddSpawn(spawn);
    }

    /// <summary>
    /// Section 2: Spot / Hotspots.
    /// Columns: MobID MapID Distance X Y [Dir] [Quantity] (or Range X Y)
    /// </summary>
    private void ParseSection2(ConversionResult result, string[] tokens, int lineNumber, string rawLine, string? comment)
    {
        if (tokens.Length < 5)
        {
            result.AddIssue(IssueSeverity.Error, lineNumber, rawLine,
                $"Section 2 line requires at least 5 columns, found {tokens.Length}.");
            return;
        }

        // Section 2 in Webzen files is often: MobID MapID Range X Y Dir Quantity
        if (!TryParseInt(tokens[0], out int mobId) ||
            !TryParseShort(tokens[1], out short mapId) ||
            !TryParseInt(tokens[2], out int range) ||
            !TryParseInt(tokens[3], out int x) ||
            !TryParseInt(tokens[4], out int y))
        {
            result.AddIssue(IssueSeverity.Error, lineNumber, rawLine, "Section 2 contains non-numeric column data.");
            return;
        }

        int webzenDir = -1;
        if (tokens.Length >= 6 && TryParseInt(tokens[5], out int parsedDir))
        {
            webzenDir = parsedDir;
        }

        short quantity = 1;
        if (tokens.Length >= 7 && TryParseShort(tokens[6], out short parsedQty))
        {
            quantity = parsedQty <= 0 ? (short)1 : parsedQty;
        }

        int x1 = range > 0 ? Math.Max(0, x - range) : x;
        int y1 = range > 0 ? Math.Max(0, y - range) : y;
        int x2 = range > 0 ? Math.Min(255, x + range) : x;
        int y2 = range > 0 ? Math.Min(255, y + range) : y;

        if (!ValidateCoordinates(result, lineNumber, rawLine, x1, y1, x2, y2, out byte bX1, out byte bY1, out byte bX2, out byte bY2))
        {
            return;
        }

        ValidateMetadata(result, lineNumber, rawLine, mobId, mapId);

        var spawn = new SpawnArea
        {
            Id = Guid.NewGuid(),
            MonsterNumber = mobId,
            MapNumber = mapId,
            Distance = range,
            X1 = bX1,
            Y1 = bY1,
            X2 = bX2,
            Y2 = bY2,
            Direction = DirectionHelper.FromWebzenDirection(webzenDir),
            Quantity = quantity,
            SpawnTrigger = SpawnTrigger.Automatic,
            WaveNumber = 0,
            SpawnType = SpawnType.PointSpot,
            SourceLineNumber = lineNumber,
            Comment = comment
        };

        result.AddSpawn(spawn);
    }

    private bool ValidateCoordinates(
        ConversionResult result,
        int lineNumber,
        string rawLine,
        int x1, int y1, int x2, int y2,
        out byte bX1, out byte bY1, out byte bX2, out byte bY2)
    {
        bX1 = bY1 = bX2 = bY2 = 0;

        // Coordinate out of bounds check
        if (x1 < 0 || x1 > 255 || y1 < 0 || y1 > 255 || x2 < 0 || x2 > 255 || y2 < 0 || y2 > 255)
        {
            result.AddIssue(
                IssueSeverity.Error,
                lineNumber,
                rawLine,
                $"Coordinates out of bounds: ({x1},{y1})-({x2},{y2}). Valid range is 0-255.");
            return false;
        }

        // Automatic inversion fix if x1 > x2 or y1 > y2
        if (x1 > x2)
        {
            result.AddIssue(
                IssueSeverity.Warning,
                lineNumber,
                rawLine,
                $"X1 ({x1}) was greater than X2 ({x2}). Automatically corrected.");
            (x1, x2) = (x2, x1);
        }

        if (y1 > y2)
        {
            result.AddIssue(
                IssueSeverity.Warning,
                lineNumber,
                rawLine,
                $"Y1 ({y1}) was greater than Y2 ({y2}). Automatically corrected.");
            (y1, y2) = (y2, y1);
        }

        bX1 = (byte)x1;
        bY1 = (byte)y1;
        bX2 = (byte)x2;
        bY2 = (byte)y2;
        return true;
    }

    private void ValidateMetadata(ConversionResult result, int lineNumber, string rawLine, int mobId, short mapId)
    {
        if (!StandardGameMaps.TryGetMapName(mapId, out _))
        {
            result.AddIssue(
                IssueSeverity.Warning,
                lineNumber,
                rawLine,
                $"Map ID {mapId} is not in standard MU Online map table (custom map detected).");
        }

        if (!StandardMonsters.IsKnownMonster(mobId))
        {
            result.AddIssue(
                IssueSeverity.Warning,
                lineNumber,
                rawLine,
                $"Monster ID {mobId} is not in standard MU Online monster table (custom monster detected).");
        }
    }

    private static bool TryParseInt(string value, out int result)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseShort(string value, out short result)
    {
        return short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }
}
