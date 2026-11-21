namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Root DTO matching OpenMU Map Editor import/export schema.
/// </summary>
public sealed record MapSpawnExport
{
    /// <summary>
    /// Format version expected by OpenMU MapExportImportService ("1.0").
    /// </summary>
    public string FormatVersion { get; set; } = "1.0";

    /// <summary>Exported monster and NPC spawn areas.</summary>
    public IList<SpawnExport> Spawns { get; set; } = new List<SpawnExport>();

    /// <summary>Exported exit gates.</summary>
    public IList<ExitGateExport> ExitGates { get; set; } = new List<ExitGateExport>();

    /// <summary>Exported enter gates.</summary>
    public IList<EnterGateExport> EnterGates { get; set; } = new List<EnterGateExport>();
}

/// <summary>
/// Serializable monster spawn area matching OpenMU's SpawnExport DTO.
/// </summary>
public sealed record SpawnExport
{
    public Guid Id { get; set; }
    public byte X1 { get; set; }
    public byte Y1 { get; set; }
    public byte X2 { get; set; }
    public byte Y2 { get; set; }
    public Direction Direction { get; set; }
    public short Quantity { get; set; }
    public SpawnTrigger SpawnTrigger { get; set; }
    public byte WaveNumber { get; set; }
    public int? MaximumHealthOverride { get; set; }
    public int MonsterNumber { get; set; }
}

/// <summary>
/// Serializable exit gate matching OpenMU's ExitGateExport DTO.
/// </summary>
public sealed record ExitGateExport
{
    public Guid Id { get; set; }
    public byte X1 { get; set; }
    public byte Y1 { get; set; }
    public byte X2 { get; set; }
    public byte Y2 { get; set; }
    public Direction Direction { get; set; }
    public bool IsSpawnGate { get; set; }
}

/// <summary>
/// Serializable enter gate matching OpenMU's EnterGateExport DTO.
/// </summary>
public sealed record EnterGateExport
{
    public Guid Id { get; set; }
    public byte X1 { get; set; }
    public byte Y1 { get; set; }
    public byte X2 { get; set; }
    public byte Y2 { get; set; }
    public short LevelRequirement { get; set; }
    public short Number { get; set; }
    public Guid? TargetGateId { get; set; }
}
