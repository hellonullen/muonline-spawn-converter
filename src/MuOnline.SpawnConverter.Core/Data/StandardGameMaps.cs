namespace MuOnline.SpawnConverter.Core.Data;

/// <summary>
/// Registry of standard Webzen / OpenMU map identifiers and names.
/// </summary>
public static class StandardGameMaps
{
    private static readonly Dictionary<short, string> Maps = new()
    {
        { 0, "Lorencia" },
        { 1, "Dungeon" },
        { 2, "Devias" },
        { 3, "Noria" },
        { 4, "Lost Tower" },
        { 5, "Exile" },
        { 6, "Arena" },
        { 7, "Atlans" },
        { 8, "Tarkan" },
        { 9, "Devil Square" },
        { 10, "Icarus" },
        { 11, "Blood Castle 1" },
        { 12, "Blood Castle 2" },
        { 13, "Blood Castle 3" },
        { 14, "Blood Castle 4" },
        { 15, "Blood Castle 5" },
        { 16, "Blood Castle 6" },
        { 17, "Blood Castle 7" },
        { 18, "Chaos Castle 1" },
        { 19, "Chaos Castle 2" },
        { 20, "Chaos Castle 3" },
        { 21, "Chaos Castle 4" },
        { 22, "Chaos Castle 5" },
        { 23, "Chaos Castle 6" },
        { 24, "Kalima 1" },
        { 25, "Kalima 2" },
        { 26, "Kalima 3" },
        { 27, "Kalima 4" },
        { 28, "Kalima 5" },
        { 29, "Kalima 6" },
        { 30, "Valley of Loren" },
        { 31, "Land of Trials" },
        { 32, "Devil Square Master" },
        { 33, "Aida" },
        { 34, "Crywolf Fortress" },
        { 36, "Kalima 7" },
        { 37, "Kanturu Ruins" },
        { 38, "Kanturu Relics" },
        { 39, "Tower of Refinement" },
        { 40, "Silent Map" },
        { 41, "Barracks of Balgass" },
        { 42, "Balgass Refuge" },
        { 45, "Illusion Temple 1" },
        { 46, "Illusion Temple 2" },
        { 47, "Illusion Temple 3" },
        { 48, "Illusion Temple 4" },
        { 49, "Illusion Temple 5" },
        { 50, "Illusion Temple 6" },
        { 51, "Elbeland" },
        { 52, "Blood Castle 8" },
        { 53, "Chaos Castle 7" },
        { 56, "Swamp of Calmness" },
        { 57, "Raklion" },
        { 58, "Raklion Boss (Hatchery)" },
        { 62, "Santa Village" },
        { 63, "Vulcanus" },
        { 64, "Duel Arena" },
        { 65, "Doppelganger 1" },
        { 66, "Doppelganger 2" },
        { 67, "Doppelganger 3" },
        { 68, "Doppelganger 4" },
        { 69, "Imperial Guardian 1" },
        { 70, "Imperial Guardian 2" },
        { 71, "Imperial Guardian 3" },
        { 72, "Imperial Guardian 4" },
        { 79, "Loren Market" },
        { 80, "Karutan 1" },
        { 81, "Karutan 2" },
    };

    public static bool TryGetMapName(short mapNumber, out string name)
    {
        return Maps.TryGetValue(mapNumber, out name!);
    }

    public static string GetMapName(short mapNumber)
    {
        return Maps.TryGetValue(mapNumber, out var name) ? name : $"Map_{mapNumber:D2}";
    }

    public static string GetSafeFilename(short mapNumber)
    {
        var rawName = GetMapName(mapNumber);
        var cleanName = string.Join("_", rawName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                              .Replace(" ", "_");
        return $"{mapNumber:D2}_{cleanName}";
    }
}
