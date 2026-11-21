namespace MuOnline.SpawnConverter.Tests;

using MuOnline.SpawnConverter.Core.Exporters;
using MuOnline.SpawnConverter.Core.Models;
using MuOnline.SpawnConverter.Core.Parsers;
using Xunit;

public class RoundTripTests
{
    [Fact]
    public void RoundTrip_MonsterSetBase_Parse_Export_ParseAgain_ShouldPreserveData()
    {
        string originalMsb = """
        0
        240  0  0  125  125  2  // Safety Guard
        end

        1
        0  0  30  130  130  160  160  -1  15  // Bull Fighter
        end
        """;

        var parser = new MonsterSetBaseParser();
        var exporter = new MonsterSetBaseExporter();

        // 1. Initial Parse
        var result1 = parser.Parse(originalMsb);
        Assert.Equal(2, result1.Spawns.Count);

        // 2. Export back to MSB format
        string exportedMsb = exporter.Export(result1.Spawns);
        Assert.NotNull(exportedMsb);

        // 3. Re-parse the generated MSB text
        var result2 = parser.Parse(exportedMsb);
        Assert.Equal(2, result2.Spawns.Count);

        // 4. Assert all properties match
        var s1_a = result1.Spawns[0];
        var s2_a = result2.Spawns[0];
        Assert.Equal(s1_a.MonsterNumber, s2_a.MonsterNumber);
        Assert.Equal(s1_a.MapNumber, s2_a.MapNumber);
        Assert.Equal(s1_a.X1, s2_a.X1);
        Assert.Equal(s1_a.Y1, s2_a.Y1);
        Assert.Equal(s1_a.Direction, s2_a.Direction);
        Assert.Equal(s1_a.Quantity, s2_a.Quantity);

        var s1_b = result1.Spawns[1];
        var s2_b = result2.Spawns[1];
        Assert.Equal(s1_b.MonsterNumber, s2_b.MonsterNumber);
        Assert.Equal(s1_b.MapNumber, s2_b.MapNumber);
        Assert.Equal(s1_b.X1, s2_b.X1);
        Assert.Equal(s1_b.Y1, s2_b.Y1);
        Assert.Equal(s1_b.X2, s2_b.X2);
        Assert.Equal(s1_b.Y2, s2_b.Y2);
        Assert.Equal(s1_b.Quantity, s2_b.Quantity);
    }
}
