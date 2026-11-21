namespace MuOnline.SpawnConverter.Core.Exporters;

using System.Text.Json;
using MuOnline.SpawnConverter.Core.Data;
using MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Exports spawn areas into a clean, human-readable generic JSON structure for general tooling and web apps.
/// </summary>
public sealed class GenericJsonExporter : ISpawnExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string FormatName => "json";

    public string Export(IEnumerable<SpawnArea> spawns)
    {
        var records = spawns.Select(s => new
        {
            s.Id,
            monster = new
            {
                id = s.MonsterNumber,
                name = StandardMonsters.GetMonsterName(s.MonsterNumber)
            },
            map = new
            {
                id = s.MapNumber,
                name = StandardGameMaps.GetMapName(s.MapNumber)
            },
            coordinates = new
            {
                x1 = s.X1,
                y1 = s.Y1,
                x2 = s.X2,
                y2 = s.Y2,
                isPoint = s.IsPoint
            },
            direction = new
            {
                value = (int)s.Direction,
                name = s.Direction.ToString()
            },
            quantity = s.Quantity,
            distance = s.Distance,
            spawnType = s.SpawnType.ToString(),
            comment = s.Comment
        });

        return JsonSerializer.Serialize(records, JsonOptions);
    }
}
