namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Defines the trigger event condition when a monster spawns.
/// Aligned with OpenMU's SpawnTrigger enum.
/// </summary>
public enum SpawnTrigger
{
    /// <summary>The monster spawns and respawns automatically.</summary>
    Automatic = 0,

    /// <summary>The monster spawns automatically during an event.</summary>
    AutomaticDuringEvent = 1,

    /// <summary>The monster spawns just once at the beginning of an event (e.g. BC gates, golden mobs).</summary>
    OnceAtEventStart = 2,
}
