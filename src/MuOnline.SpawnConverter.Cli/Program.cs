namespace MuOnline.SpawnConverter.Cli;

using System.Text.Json;
using MuOnline.SpawnConverter.Core.Data;
using MuOnline.SpawnConverter.Core.Exporters;
using MuOnline.SpawnConverter.Core.Models;
using MuOnline.SpawnConverter.Core.Parsers;

public static class Program
{
    private const string Version = "1.0.0";

    public static int Main(string[] args)
    {
        PrintBanner();

        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            PrintUsage();
            return 0;
        }

        string command = args[0].ToLowerInvariant();
        try
        {
            return command switch
            {
                "convert" => HandleConvert(args.Skip(1).ToArray()),
                "reverse" => HandleReverse(args.Skip(1).ToArray()),
                "info" => HandleInfo(args.Skip(1).ToArray()),
                _ => HandleDirectConvertOrHelp(args)
            };
        }
        catch (Exception ex)
        {
            WriteColor(ConsoleColor.Red, $"[FATAL ERROR] {ex.Message}");
            return 1;
        }
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"  __  __ _   _    ____                                  ____                            _            ");
        Console.WriteLine(@" |  \/  | | | |  / ___| _ __   __ ___      ___ __      / ___|___  _ ____   _____ _ __| |_ ___ _ __ ");
        Console.WriteLine(@" | |\/| | | | |  \___ \| '_ \ / _` \ \ /\ / / '_ \    | |   / _ \| '_ \ \ / / _ \ '__| __/ _ \ '__|");
        Console.WriteLine(@" | |  | | |_| |   ___) | |_) | (_| |\ V  V /| | | |   | |__| (_) | | | \ V /  __/ |  | ||  __/ |   ");
        Console.WriteLine(@" |_|  |_|\___/   |____/| .__/ \__,_| \_/\_/ |_| |_|    \____\___/|_| |_|\_/ \___|_|   \__\___|_|   ");
        Console.WriteLine(@"                       |_|                                                                          ");
        Console.ResetColor();
        Console.WriteLine($"  MU Online Spawn & Spot Converter - v{Version}");
        Console.WriteLine("  Repository: https://github.com/hellonullen/muonline-spawn-converter");
        Console.WriteLine("  License: MIT | Author: hellonullen");
        Console.WriteLine("  --------------------------------------------------------------------------\n");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  mu-spawn-converter convert <input_file> [options]");
        Console.WriteLine("  mu-spawn-converter reverse <input_json> [options]");
        Console.WriteLine("  mu-spawn-converter info <input_file>\n");

        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  convert    Parse classic MonsterSetBase.txt and convert to OpenMU JSON or Generic JSON.");
        Console.WriteLine("  reverse    Convert OpenMU Map Editor JSON back to standard MonsterSetBase.txt format.");
        Console.WriteLine("  info       Inspect a MonsterSetBase.txt or JSON file and display statistical summary.\n");

        Console.WriteLine("OPTIONS (convert):");
        Console.WriteLine("  -o, --output <path>    Output directory or specific output file path (default: './output').");
        Console.WriteLine("  -f, --format <format>  Export format: 'openmu' (default), 'json', or 'all'.");
        Console.WriteLine("  --split-maps           Split OpenMU output into per-map JSON files (e.g. 00_Lorencia.json). [Enabled by default]");
        Console.WriteLine("  --no-split             Generate a single all-in-one output file instead of splitting by map.");
        Console.WriteLine("  --strict               Fail with non-zero exit code if any warnings occur.\n");

        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  # Convert MonsterSetBase.txt to OpenMU Map Editor JSON files split by map");
        Console.WriteLine("  mu-spawn-converter convert MonsterSetBase.txt -o ./openmu_spawns/");
        Console.WriteLine();
        Console.WriteLine("  # Convert MonsterSetBase.txt to single generic JSON file");
        Console.WriteLine("  mu-spawn-converter convert MonsterSetBase.txt -o spawns.json -f json --no-split");
        Console.WriteLine();
        Console.WriteLine("  # Convert OpenMU JSON back to MonsterSetBase.txt");
        Console.WriteLine("  mu-spawn-converter reverse 00_Lorencia.json -o MonsterSetBase_Lorencia.txt\n");
    }

    private static int HandleDirectConvertOrHelp(string[] args)
    {
        // If user directly passed a filename (e.g. `mu-spawn-converter MonsterSetBase.txt`)
        if (File.Exists(args[0]))
        {
            return HandleConvert(args);
        }

        WriteColor(ConsoleColor.Red, $"[ERROR] Unknown command or file not found: {args[0]}\n");
        PrintUsage();
        return 1;
    }

    private static int HandleConvert(string[] args)
    {
        if (args.Length == 0)
        {
            WriteColor(ConsoleColor.Red, "[ERROR] Missing input file path.");
            PrintUsage();
            return 1;
        }

        string inputPath = args[0];
        string outputDirOrFile = "./output";
        string format = "openmu";
        bool splitMaps = true;
        bool strict = false;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "-o" or "--output" when i + 1 < args.Length:
                    outputDirOrFile = args[++i];
                    break;
                case "-f" or "--format" when i + 1 < args.Length:
                    format = args[++i].ToLowerInvariant();
                    break;
                case "--split-maps":
                    splitMaps = true;
                    break;
                case "--no-split":
                    splitMaps = false;
                    break;
                case "--strict":
                    strict = true;
                    break;
            }
        }

        if (!File.Exists(inputPath))
        {
            WriteColor(ConsoleColor.Red, $"[ERROR] Input file not found: {inputPath}");
            return 1;
        }

        Console.WriteLine($"[INFO] Parsing input file: {Path.GetFullPath(inputPath)}");
        var parser = new MonsterSetBaseParser();
        var result = parser.ParseFile(inputPath);

        // Display Issues
        DisplayIssues(result);

        if (result.Spawns.Count == 0)
        {
            WriteColor(ConsoleColor.Red, "[ERROR] No valid spawns could be parsed from input file.");
            return 1;
        }

        // Summary
        WriteColor(ConsoleColor.Green, $"[SUCCESS] Successfully parsed {result.Spawns.Count:N0} spawns across {result.GetMapNumbers().Count()} maps.");

        // Exporting
        if (format is "openmu" or "all")
        {
            var openMuExporter = new OpenMuMapEditorExporter();
            if (splitMaps)
            {
                string targetDir = outputDirOrFile;
                if (Path.HasExtension(targetDir))
                {
                    targetDir = Path.GetDirectoryName(targetDir) ?? "./output";
                }

                Console.WriteLine($"\n[INFO] Exporting OpenMU Map Editor JSONs to directory: {Path.GetFullPath(targetDir)}");
                var created = openMuExporter.ExportSplitMaps(result, targetDir);

                foreach (var (fileName, count) in created)
                {
                    Console.WriteLine($"  - Created {fileName,-30} ({count,4} spawns)");
                }
            }
            else
            {
                string targetFile = Path.HasExtension(outputDirOrFile) ? outputDirOrFile : Path.Combine(outputDirOrFile, "openmu_all_spawns.json");
                string? dir = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                string json = openMuExporter.Export(result.Spawns);
                File.WriteAllText(targetFile, json);
                Console.WriteLine($"  - Created single OpenMU export: {Path.GetFullPath(targetFile)} ({result.Spawns.Count} spawns)");
            }
        }

        if (format is "json" or "all")
        {
            var genericExporter = new GenericJsonExporter();
            string targetFile = Path.HasExtension(outputDirOrFile) && format == "json"
                ? outputDirOrFile
                : Path.Combine(Path.HasExtension(outputDirOrFile) ? Path.GetDirectoryName(outputDirOrFile) ?? "./output" : outputDirOrFile, "generic_spawns.json");

            string? dir = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            string json = genericExporter.Export(result.Spawns);
            File.WriteAllText(targetFile, json);
            Console.WriteLine($"  - Created generic JSON export: {Path.GetFullPath(targetFile)} ({result.Spawns.Count} spawns)");
        }

        Console.WriteLine("\n[DONE] Conversion completed successfully!");

        if (strict && result.WarningCount > 0)
        {
            WriteColor(ConsoleColor.Yellow, $"[STRICT MODE] Exiting with code 2 due to {result.WarningCount} warning(s).");
            return 2;
        }

        return result.HasErrors ? 1 : 0;
    }

    private static int HandleReverse(string[] args)
    {
        if (args.Length == 0)
        {
            WriteColor(ConsoleColor.Red, "[ERROR] Missing input JSON file path.");
            PrintUsage();
            return 1;
        }

        string inputPath = args[0];
        string outputPath = args.Length > 2 && args[1] is "-o" or "--output" ? args[2] : "MonsterSetBase_converted.txt";

        if (!File.Exists(inputPath))
        {
            WriteColor(ConsoleColor.Red, $"[ERROR] File not found: {inputPath}");
            return 1;
        }

        Console.WriteLine($"[INFO] Reading OpenMU JSON file: {Path.GetFullPath(inputPath)}");
        string json = File.ReadAllText(inputPath);

        MapSpawnExport? dto;
        try
        {
            dto = JsonSerializer.Deserialize<MapSpawnExport>(json);
        }
        catch (JsonException ex)
        {
            WriteColor(ConsoleColor.Red, $"[ERROR] Invalid JSON structure: {ex.Message}");
            return 1;
        }

        if (dto is null || dto.Spawns is null || dto.Spawns.Count == 0)
        {
            WriteColor(ConsoleColor.Yellow, "[WARNING] No spawns found in input JSON.");
            return 0;
        }

        var spawns = dto.Spawns.Select(s => new SpawnArea
        {
            Id = s.Id,
            MonsterNumber = s.MonsterNumber,
            MapNumber = 0, // In individual OpenMU map exports, MapID is inferred by file or default
            X1 = s.X1,
            Y1 = s.Y1,
            X2 = s.X2,
            Y2 = s.Y2,
            Direction = s.Direction,
            Quantity = s.Quantity,
            SpawnTrigger = s.SpawnTrigger,
            WaveNumber = s.WaveNumber,
            MaximumHealthOverride = s.MaximumHealthOverride
        }).ToList();

        var exporter = new MonsterSetBaseExporter();
        string msbText = exporter.Export(spawns);

        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(outputPath, msbText);
        WriteColor(ConsoleColor.Green, $"[SUCCESS] Exported {spawns.Count} spawns to {Path.GetFullPath(outputPath)}");
        return 0;
    }

    private static int HandleInfo(string[] args)
    {
        if (args.Length == 0)
        {
            WriteColor(ConsoleColor.Red, "[ERROR] Missing file path.");
            return 1;
        }

        string inputPath = args[0];
        if (!File.Exists(inputPath))
        {
            WriteColor(ConsoleColor.Red, $"[ERROR] File not found: {inputPath}");
            return 1;
        }

        string ext = Path.GetExtension(inputPath).ToLowerInvariant();
        if (ext == ".json")
        {
            string json = File.ReadAllText(inputPath);
            var dto = JsonSerializer.Deserialize<MapSpawnExport>(json);
            if (dto is null)
            {
                WriteColor(ConsoleColor.Red, "[ERROR] Failed to deserialize JSON.");
                return 1;
            }

            Console.WriteLine($"Format Version : {dto.FormatVersion}");
            Console.WriteLine($"Total Spawns   : {dto.Spawns.Count}");
            Console.WriteLine($"Exit Gates     : {dto.ExitGates.Count}");
            Console.WriteLine($"Enter Gates    : {dto.EnterGates.Count}");
            return 0;
        }

        var parser = new MonsterSetBaseParser();
        var result = parser.ParseFile(inputPath);

        Console.WriteLine($"File Analyzed  : {Path.GetFileName(inputPath)}");
        Console.WriteLine($"Total Lines    : {result.TotalLinesRead:N0}");
        Console.WriteLine($"Valid Spawns   : {result.SuccessCount:N0}");
        Console.WriteLine($"Warnings       : {result.WarningCount:N0}");
        Console.WriteLine($"Errors         : {result.ErrorCount:N0}");
        Console.WriteLine($"Unique Maps    : {result.GetMapNumbers().Count()}");

        Console.WriteLine("\nMap Breakdown:");
        foreach (var map in result.GetMapNumbers())
        {
            var count = result.GetSpawnsForMap(map).Count;
            Console.WriteLine($"  - [{map,2}] {StandardGameMaps.GetMapName(map),-25}: {count,5} spawns");
        }

        return 0;
    }

    private static void DisplayIssues(ConversionResult result)
    {
        if (result.Issues.Count == 0)
        {
            return;
        }

        Console.WriteLine("\n--- Parser Diagnostics & Notices ---");
        foreach (var issue in result.Issues)
        {
            var color = issue.Severity switch
            {
                IssueSeverity.Error => ConsoleColor.Red,
                IssueSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.DarkGray
            };

            WriteColor(color, $"  {issue}");
        }
        Console.WriteLine("------------------------------------\n");
    }

    private static void WriteColor(ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
