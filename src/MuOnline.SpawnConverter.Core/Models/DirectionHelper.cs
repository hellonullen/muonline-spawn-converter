namespace MuOnline.SpawnConverter.Core.Models;

/// <summary>
/// Helper methods for mapping between Webzen MonsterSetBase directions and OpenMU directions.
/// </summary>
public static class DirectionHelper
{
    /// <summary>
    /// Converts a classic Webzen MonsterSetBase direction integer (0-7, or -1/other) into OpenMU Direction.
    /// In Webzen: 0=West, 1=SW, 2=South, 3=SE, 4=East, 5=NE, 6=North, 7=NW, -1=Random/Undefined.
    /// In OpenMU: 0=Undefined, 1=West, 2=SW, 3=South, 4=SE, 5=East, 6=NE, 7=North, 8=NW.
    /// </summary>
    public static Direction FromWebzenDirection(int webzenDir)
    {
        return webzenDir switch
        {
            0 => Direction.West,
            1 => Direction.SouthWest,
            2 => Direction.South,
            3 => Direction.SouthEast,
            4 => Direction.East,
            5 => Direction.NorthEast,
            6 => Direction.North,
            7 => Direction.NorthWest,
            _ => Direction.Undefined
        };
    }

    /// <summary>
    /// Converts an OpenMU Direction back into a classic Webzen direction integer.
    /// </summary>
    public static int ToWebzenDirection(Direction direction)
    {
        return direction switch
        {
            Direction.West => 0,
            Direction.SouthWest => 1,
            Direction.South => 2,
            Direction.SouthEast => 3,
            Direction.East => 4,
            Direction.NorthEast => 5,
            Direction.North => 6,
            Direction.NorthWest => 7,
            _ => -1
        };
    }
}
