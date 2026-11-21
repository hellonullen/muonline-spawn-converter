namespace MuOnline.SpawnConverter.Tests;

using System.Text.Json;
using MuOnline.SpawnConverter.Core.Exporters;
using MuOnline.SpawnConverter.Core.Models;
using Xunit;

public class OpenMuExporterTests
{
    private readonly OpenMuMapEditorExporter _exporter = new();

    [Fact]
    public void Export_ShouldProduceValidOpenMuJsonSchema()
    {
        var spawns = new List<SpawnArea>
        {
            new()
            {
                Id = Guid.NewGuid(),
                MonsterNumber = 240,
                MapNumber = 0,
                X1 = 125,
                Y1 = 125,
                X2 = 125,
                Y2 = 125,
                Direction = Direction.South,
                Quantity = 1,
                SpawnTrigger = SpawnTrigger.Automatic,
                WaveNumber = 0
            }
        };

        string json = _exporter.Export(spawns);
        Assert.NotNull(json);

        var deserialized = JsonSerializer.Deserialize<MapSpawnExport>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("1.0", deserialized.FormatVersion);
        Assert.Single(deserialized.Spawns);

        var spawnDto = deserialized.Spawns[0];
        Assert.Equal(240, spawnDto.MonsterNumber);
        Assert.Equal(125, spawnDto.X1);
        Assert.Equal(125, spawnDto.Y1);
        Assert.Equal(Direction.South, spawnDto.Direction);
        Assert.Equal(1, spawnDto.Quantity);
        Assert.Equal(SpawnTrigger.Automatic, spawnDto.SpawnTrigger);
    }

    [Fact]
    public void DirectionMapping_WebzenToOpenMu_MatchesExactEnums()
    {
        // Webzen 0 (West) => OpenMU West (1)
        Assert.Equal(Direction.West, DirectionHelper.FromWebzenDirection(0));
        Assert.Equal(0, DirectionHelper.ToWebzenDirection(Direction.West));

        // Webzen 2 (South) => OpenMU South (3)
        Assert.Equal(Direction.South, DirectionHelper.FromWebzenDirection(2));
        Assert.Equal(2, DirectionHelper.ToWebzenDirection(Direction.South));

        // Webzen 6 (North) => OpenMU North (7)
        Assert.Equal(Direction.North, DirectionHelper.FromWebzenDirection(6));
        Assert.Equal(6, DirectionHelper.ToWebzenDirection(Direction.North));

        // Webzen -1 => OpenMU Undefined (0)
        Assert.Equal(Direction.Undefined, DirectionHelper.FromWebzenDirection(-1));
    }
}
