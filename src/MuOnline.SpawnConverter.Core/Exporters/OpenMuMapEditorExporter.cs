namespace MuOnline.SpawnConverter.Core.Exporters;

using System.Text.Json;
using MuOnline.SpawnConverter.Core.Data;
using MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Exports spawn areas into the exact JSON format required by OpenMU's Map Editor.
/// </summary>
public sealed class OpenMuMapEditorExporter : ISpawnExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string FormatName => "openmu";

    public string Export(IEnumerable<SpawnArea> spawns)
    {
        var exportDto = BuildDto(spawns);
        return JsonSerializer.Serialize(exportDto, JsonOptions);
    }

    /// <summary>
    /// Exports spawns split into separate files per map in the given output directory.
    /// Each file is named like '00_Lorencia.json', '02_Devias.json', etc.
    /// </summary>
    /// <returns>A dictionary of created filenames and their spawn count.</returns>
    public IReadOnlyDictionary<string, int> ExportSplitMaps(ConversionResult result, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var createdFiles = new Dictionary<string, int>();

        foreach (var mapNumber in result.GetMapNumbers())
        {
            var mapSpawns = result.GetSpawnsForMap(mapNumber);
            if (mapSpawns.Count == 0)
            {
                continue;
            }

            string fileName = $"{StandardGameMaps.GetSafeFilename(mapNumber)}.json";
            string fullPath = Path.Combine(outputDirectory, fileName);

            string json = Export(mapSpawns);
            File.WriteAllText(fullPath, json);

            createdFiles[fileName] = mapSpawns.Count;
        }

        return createdFiles;
    }

    public static MapSpawnExport BuildDto(IEnumerable<SpawnArea> spawns)
    {
        var dto = new MapSpawnExport
        {
            FormatVersion = "1.0"
        };

        foreach (var s in spawns)
        {
            dto.Spawns.Add(new SpawnExport
            {
                Id = s.Id == Guid.Empty ? Guid.NewGuid() : s.Id,
                MonsterNumber = s.MonsterNumber,
                X1 = s.X1,
                Y1 = s.Y1,
                X2 = s.X2,
                Y2 = s.Y2,
                Direction = s.Direction,
                Quantity = s.Quantity,
                SpawnTrigger = s.SpawnTrigger,
                WaveNumber = s.WaveNumber,
                MaximumHealthOverride = s.MaximumHealthOverride
            });
        }

        return dto;
    }
}
