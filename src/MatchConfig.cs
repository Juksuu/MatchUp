using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public static class MatchConfig
{
    public static int PlayersPerTeam { get; private set; } = 5;
    public static bool KnifeRound { get; private set; } = true;
    public static string[] MapPool { get; private set; } = [];
    public static Dictionary<string, string> Settings { get; } = new();

    private static string _map = "de_mirage";

    private static readonly string[] DefaultMapPool =
        ["de_ancient", "de_anubis", "de_dust2", "de_inferno", "de_mirage", "de_nuke", "de_train"];

    public static void LoadMaps()
    {
        // Check environment variable first for maps, then local file and lastly,
        // use the default map options specified in this class
        var envMaps = Environment.GetEnvironmentVariable("MATCHUP_MAPS");
        if (envMaps != null)
        {
            Console.WriteLine("Using map pool from MATCHUP_MAPS env variable");
            MapPool = envMaps.Split(",").Where(Server.IsMapValid).ToArray();
            return;
        }

        var mapFile = Path.Combine(Server.GameDirectory + "/csgo/cfg/MatchUp", "maps.txt");
        if (File.Exists(mapFile))
        {
            Console.WriteLine("Using map pool from maps.txt");
            var mapsRaw = File.ReadLines(mapFile);
            MapPool = mapsRaw.Where(Server.IsMapValid).ToArray();
            return;
        }

        Console.WriteLine("Using default map pool");
        MapPool = DefaultMapPool;
    }

    public static void LoadSettings()
    {
        // reset settings
        Settings.Clear();
        // start loading settings
        var settingsFile = Path.Combine(Server.GameDirectory + "/csgo/cfg/MatchUp", "settings.txt");
        if (!File.Exists(settingsFile))
        {
            Console.WriteLine("Using default settings.");
            return;
        }

        Console.WriteLine("Using settings from settings.txt");
        var lines = File.ReadLines(settingsFile);
        foreach (var line in lines)
        {
            // Make sure the line is not empty and not commented out.
            if (string.IsNullOrEmpty(line)) continue;
            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith(';')) continue;
            // Split the line in a maximum of 2 parts, making sure that values including equal signs are left intact.
            // Empty values or values only containing whitespace are ignored.
            var parts = trimmedLine.Split('=', 2,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Settings[key] = value;
                Console.WriteLine($"Loaded setting: {key} -> {value}");
            }
            else
            {
                // there was no equal sign in the line, or the value was empty or only contained whitespaces.
                Console.WriteLine($"Malformed setting (missing = or value): {trimmedLine}");
            }
        }

        Console.WriteLine($"Loaded {Settings.Count} settings from settings.txt");
    }

    public static void Print(int? userid = null)
    {
        Action<string> printMessage;
        if (!userid.HasValue)
        {
            printMessage = Server.PrintToChatAll;
        }
        else
        {
            var player = Utilities.GetPlayerFromUserid(userid.Value);
            if (player == null)
            {
                return;
            }

            printMessage = player.PrintToChat;
        }

        printMessage($" {ChatColors.Green} Current config");
        printMessage($" {ChatColors.Grey} Map: {ChatColors.Gold} {_map}");
        printMessage($" {ChatColors.Grey} Players per team: {ChatColors.Gold} {PlayersPerTeam}");
        printMessage($" {ChatColors.Grey} Knife round enabled: {ChatColors.Gold} {KnifeRound}");
    }

    public static bool SetMap(string? map, CCSPlayerController? player = null)
    {
        if (map == null || !Server.IsMapValid(map))
        {
            return false;
        }

        Console.WriteLine($"Setting map to {map}");
        if (player != null)
        {
            player.PrintToChat($" Setting map to: {map}");
        }

        _map = map;

        return true;
    }

    public static bool SetTeamSize(string? teamSize, CCSPlayerController? player = null)
    {
        if (teamSize == null || !int.TryParse(teamSize, out var parsedTeamSize))
        {
            return false;
        }

        Console.WriteLine($"Setting team size to {parsedTeamSize}");
        if (player != null)
        {
            player.PrintToChat($" Setting team size to: {teamSize}");
        }

        PlayersPerTeam = parsedTeamSize;

        return true;
    }

    public static bool SetKnife(string? knife, CCSPlayerController? player = null)
    {
        if (knife == null || !bool.TryParse(knife, out var parsedKnifeRound))
        {
            return false;
        }

        Console.WriteLine($"Setting knife round to: {parsedKnifeRound}");
        if (player != null)
        {
            player.PrintToChat($" Setting knife round to: {parsedKnifeRound}");
        }

        KnifeRound = parsedKnifeRound;

        return true;
    }

    public static void StartMatch()
    {
        Server.PrintToChatAll($" {ChatColors.Green}Setting up match with current config");
        Print();

        if (Server.MapName == _map)
        {
            Server.ExecuteCommand("mp_restartgame 1");
            StateMachine.SwitchState(GameState.ReadyUp);
        }
        else
        {
            Server.ExecuteCommand($"changelevel {_map}");
        }
    }
}