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

    private static string[] defaultMapPool = { "de_ancient", "de_anubis", "de_dust2", "de_inferno", "de_mirage", "de_nuke", "de_train" };

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
            StateMachine.SwitchState(GameState.Readyup);
        }
        else
        {
            Server.ExecuteCommand($"changelevel {MatchConfig.map}");
        }

    }
}
