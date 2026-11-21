namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Direction where a character or monster is looking at.
/// Aligned with OpenMU's Direction enum model.
/// </summary>
public enum Direction
{
    /// <summary>Undefined or random direction.</summary>
    Undefined = 0,

    /// <summary>West (Webzen 0).</summary>
    West = 1,

    /// <summary>South-West (Webzen 1).</summary>
    SouthWest = 2,

    /// <summary>South (Webzen 2).</summary>
    South = 3,

    /// <summary>South-East (Webzen 3).</summary>
    SouthEast = 4,

    /// <summary>East (Webzen 4).</summary>
    East = 5,

    /// <summary>North-East (Webzen 5).</summary>
    NorthEast = 6,

    /// <summary>North (Webzen 6).</summary>
    North = 7,

    /// <summary>North-West (Webzen 7).</summary>
    NorthWest = 8,
}
