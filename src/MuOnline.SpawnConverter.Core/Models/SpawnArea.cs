namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Canonical domain model representing a monster or NPC spawn definition.
/// Compatible with both classic MU Online logic and OpenMU's MonsterSpawnArea.
/// </summary>
public sealed class SpawnArea
{
    /// <summary>Unique identifier for OpenMU persistence.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Monster or NPC index from standard Monster.txt (0 - 503+).</summary>
    public int MonsterNumber { get; set; }

    /// <summary>Map index where the spawn belongs (0 = Lorencia, 2 = Devias, etc.).</summary>
    public short MapNumber { get; set; }

    /// <summary>Top-left X coordinate (0-255).</summary>
    public byte X1 { get; set; }

    /// <summary>Top-left Y coordinate (0-255).</summary>
    public byte Y1 { get; set; }

    /// <summary>Bottom-right X coordinate (0-255). Equals X1 for static single point spawns.</summary>
    public byte X2 { get; set; }

    /// <summary>Bottom-right Y coordinate (0-255). Equals Y1 for static single point spawns.</summary>
    public byte Y2 { get; set; }

    /// <summary>Looking direction of the spawn.</summary>
    public Direction Direction { get; set; } = Direction.Undefined;

    /// <summary>Spawn quantity in this area or point (default 1).</summary>
    public short Quantity { get; set; } = 1;

    /// <summary>Spawn trigger rule.</summary>
    public SpawnTrigger SpawnTrigger { get; set; } = SpawnTrigger.Automatic;

    /// <summary>Wave number if part of an event.</summary>
    public byte WaveNumber { get; set; } = 0;

    /// <summary>Optional maximum health override.</summary>
    public int? MaximumHealthOverride { get; set; }

    /// <summary>Origin classification from MonsterSetBase sections.</summary>
    public SpawnType SpawnType { get; set; } = SpawnType.SingleNpcOrMonster;

    /// <summary>Wandering distance parameter from classic server files.</summary>
    public int Distance { get; set; }

    /// <summary>Source line number in the original MonsterSetBase.txt file.</summary>
    public int SourceLineNumber { get; set; }

    /// <summary>Optional inline or block comment associated with this spawn.</summary>
    public string? Comment { get; set; }

    /// <summary>Determines if this spawn represents a single point rather than a bounding box.</summary>
    public bool IsPoint => X1 == X2 && Y1 == Y2;

    public override string ToString()
    {
        return $"[Mob {MonsterNumber} | Map {MapNumber} | ({X1},{Y1})-({X2},{Y2}) | Qty:{Quantity} | Dir:{Direction}]";
    }
}
