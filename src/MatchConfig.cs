using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;
public static class MatchConfig
{
    public static string[] mapPool = { "de_ancient", "de_anubis", "de_inferno", "de_mirage", "de_nuke", "de_overpass", "de_vertigo" };

    // public static int maps = 1;
    public static int playersPerTeam = 5;
    public static string map = "de_mirage";

    public static void print(int? userid = null)
    {
        if (userid.HasValue)
        {
            var player = Utilities.GetPlayerFromUserid(userid.Value);
            player.PrintToChat($" {ChatColors.Green} Current config");
            player.PrintToChat($" {ChatColors.Grey} Map: {ChatColors.Gold} {map}");
            player.PrintToChat($" {ChatColors.Grey} Players per team: {ChatColors.Gold} {playersPerTeam}");
        }
        else
        {
            Server.PrintToChatAll($" {ChatColors.Green} Current config");
            Server.PrintToChatAll($" {ChatColors.Grey} Map: {ChatColors.Gold} {map}");
            Server.PrintToChatAll($" {ChatColors.Grey} Players per team: {ChatColors.Gold} {playersPerTeam}");
        }
    }
}
