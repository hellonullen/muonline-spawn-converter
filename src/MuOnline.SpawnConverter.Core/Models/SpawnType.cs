namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Source spawn classification from classic MonsterSetBase.txt sections.
/// </summary>
public enum SpawnType
{
    /// <summary>Section 0: Static single NPC or Boss spawn.</summary>
    SingleNpcOrMonster = 0,

    /// <summary>Section 1: Ranged roaming monster area.</summary>
    AreaSpawn = 1,

    /// <summary>Section 2: Clustered spot / point spot spawn.</summary>
    PointSpot = 2,

    /// <summary>Section 3: Event specific spawn.</summary>
    EventSpawn = 3,

    /// <summary>Section 4+: Custom / Extended section spawn.</summary>
    Custom = 4
}
