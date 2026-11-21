namespace MuOnline.SpawnConverter.Tests;

using MuOnline.SpawnConverter.Core.Models;
using MuOnline.SpawnConverter.Core.Parsers;
using Xunit;

public class MonsterSetBaseParserTests
{
    private readonly MonsterSetBaseParser _parser = new();

    [Fact]
    public void Parse_Section0_ShouldParseSingleNpc()
    {
        string input = """
        // Section 0
        0
        240  0  0  125  125  2  // Safety Guard Lorencia
        end
        """;

        var result = _parser.Parse(input);

        Assert.Single(result.Spawns);
        var spawn = result.Spawns[0];
        Assert.Equal(240, spawn.MonsterNumber);
        Assert.Equal(0, spawn.MapNumber);
        Assert.Equal(125, spawn.X1);
        Assert.Equal(125, spawn.Y1);
        Assert.Equal(125, spawn.X2);
        Assert.Equal(125, spawn.Y2);
        Assert.Equal(Direction.South, spawn.Direction); // Webzen 2 = South = OpenMU 3
        Assert.Equal(1, spawn.Quantity);
        Assert.True(spawn.IsPoint);
    }

    [Fact]
    public void Parse_Section1_ShouldParseAreaSpawn()
    {
        string input = """
        1
        0  0  30  130  130  150  160  -1  10  // Bull Fighter
        end
        """;

        var result = _parser.Parse(input);

        Assert.Single(result.Spawns);
        var spawn = result.Spawns[0];
        Assert.Equal(0, spawn.MonsterNumber);
        Assert.Equal(0, spawn.MapNumber);
        Assert.Equal(130, spawn.X1);
        Assert.Equal(130, spawn.Y1);
        Assert.Equal(150, spawn.X2);
        Assert.Equal(160, spawn.Y2);
        Assert.Equal(Direction.Undefined, spawn.Direction); // Webzen -1 = Undefined
        Assert.Equal(10, spawn.Quantity);
        Assert.False(spawn.IsPoint);
    }

    [Fact]
    public void Parse_Section2_ShouldParseSpotAndCalculateBounds()
    {
        string input = """
        2
        2  0  3  100  100  -1  5  // Budge Dragon spot
        end
        """;

        var result = _parser.Parse(input);

        Assert.Single(result.Spawns);
        var spawn = result.Spawns[0];
        Assert.Equal(2, spawn.MonsterNumber);
        Assert.Equal(0, spawn.MapNumber);
        Assert.Equal(97, spawn.X1);  // 100 - 3
        Assert.Equal(97, spawn.Y1);  // 100 - 3
        Assert.Equal(103, spawn.X2); // 100 + 3
        Assert.Equal(103, spawn.Y2); // 100 + 3
        Assert.Equal(5, spawn.Quantity);
    }

    [Fact]
    public void Parse_CoordinatesOutOfBounds_ShouldReportError()
    {
        string input = """
        0
        240  0  0  300  125  2
        end
        """;

        var result = _parser.Parse(input);

        Assert.Empty(result.Spawns);
        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Severity == IssueSeverity.Error && i.Message.Contains("out of bounds"));
    }

    [Fact]
    public void Parse_InvertedCoordinates_ShouldAutoCorrectAndWarn()
    {
        string input = """
        1
        0  0  30  150  130  120  140  -1  5
        end
        """;

        var result = _parser.Parse(input);

        Assert.Single(result.Spawns);
        var spawn = result.Spawns[0];
        Assert.Equal(120, spawn.X1);
        Assert.Equal(150, spawn.X2);
        Assert.Contains(result.Issues, i => i.Severity == IssueSeverity.Warning && i.Message.Contains("automatically corrected", StringComparison.OrdinalIgnoreCase));
    }
}
