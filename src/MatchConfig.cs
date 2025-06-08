using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public static class MatchConfig
{
    public static int playersPerTeam = 5;
    public static bool knifeRound = true;
    public static string map = "de_mirage";
    public static string[] mapPool = { };
    public static Dictionary<string, string> settings = new Dictionary<string, string>();

    private static string[] defaultMapPool =
        { "de_ancient", "de_anubis", "de_dust2", "de_inferno", "de_mirage", "de_nuke", "de_train" };

    public static void loadMaps()
    {
        // Check environment variable first for maps, then local file and lastly use
        // default map options specified in this class
        string? envMaps = Environment.GetEnvironmentVariable("MATCHUP_MAPS");
        if (envMaps != null)
        {
            Console.WriteLine("Using map pool from MATCHUP_MAPS env variable");
            MatchConfig.mapPool = envMaps.Split(",").Where(m => Server.IsMapValid(m)).ToArray();
        }
        else
        {
            string mapFile = Path.Combine(Server.GameDirectory + "/csgo/cfg/MatchUp", "maps.txt");
            if (File.Exists(mapFile))
            {
                Console.WriteLine("Using map pool from maps.txt");
                var mapsRaw = File.ReadLines(mapFile);
                MatchConfig.mapPool = mapsRaw.Where(m => Server.IsMapValid(m)).ToArray();
            }
            else
            {
                Console.WriteLine("Using default map pool");
                MatchConfig.mapPool = MatchConfig.defaultMapPool;
            }
        }
    }

    public static void loadSettings()
    {
        // reset settings
        settings.Clear();
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
                settings[key] = value;
                Console.WriteLine($"Loaded setting: {key} -> {value}");
            }
            else
            {
                // there was no equal sign in the line, or the value was empty or only contained whitespaces.
                Console.WriteLine($"Malformed setting (missing = or value): {trimmedLine}");
            }
        }

        Console.WriteLine($"Loaded {settings.Count} settings from settings.txt");
    }

    public static void print(int? userid = null)
    {
        if (userid.HasValue)
        {
            var player = Utilities.GetPlayerFromUserid(userid.Value);
            if (player != null)
            {
                player.PrintToChat($" {ChatColors.Green} Current config");
                player.PrintToChat($" {ChatColors.Grey} Map: {ChatColors.Gold} {map}");
                player.PrintToChat($" {ChatColors.Grey} Players per team: {ChatColors.Gold} {playersPerTeam}");
                player.PrintToChat($" {ChatColors.Grey} Knife round enabled: {ChatColors.Gold} {knifeRound}");
            }
        }
        else
        {
            Server.PrintToChatAll($" {ChatColors.Green} Current config");
            Server.PrintToChatAll($" {ChatColors.Grey} Map: {ChatColors.Gold} {map}");
            Server.PrintToChatAll($" {ChatColors.Grey} Players per team: {ChatColors.Gold} {playersPerTeam}");
            Server.PrintToChatAll($" {ChatColors.Grey} Knife round enabled: {ChatColors.Gold} {knifeRound}");
        }
    }

    public static bool setMap(string? map, CCSPlayerController? player = null)
    {
        if (map != null && Server.IsMapValid(map))
        {
            Console.WriteLine($"Setting map to {map}");
            if (player != null)
            {
                player.PrintToChat($"Setting map to: {map}");
            }

            MatchConfig.map = map;

            return true;
        }

        return false;
    }

    public static bool setTeamSize(string? teamSize, CCSPlayerController? player = null)
    {
        var result = 0;
        if (teamSize != null && Int32.TryParse(teamSize, out result))
        {
            Console.WriteLine($"Setting team size to {result}");
            if (player != null)
            {
                player.PrintToChat($"Setting team size to: {teamSize}");
            }

            MatchConfig.playersPerTeam = result;

            return true;
        }

        return false;
    }

    public static bool setKnife(string? knife, CCSPlayerController? player = null)
    {
        var result = true;
        if (knife != null && Boolean.TryParse(knife, out result))
        {
            Console.WriteLine($"Setting knife round to: {result}");
            if (player != null)
            {
                player.PrintToChat($"Setting knife round to: {result}");
            }

            MatchConfig.knifeRound = result;

            return true;
        }

        return false;
    }

    public static void StartMatch()
    {
        Server.PrintToChatAll($" {ChatColors.Green}Setting up match with current config");
        MatchConfig.print();
        if (Server.MapName == MatchConfig.map)
        {
            Server.ExecuteCommand("mp_restartgame 1");
            StateMachine.SwitchState(GameState.ReadyUp);
        }
        else
        {
            Server.ExecuteCommand($"changelevel {MatchConfig.map}");
        }
    }
}