using System.Text.Json;
using System.Text.Json.Nodes;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

/*
 * EventBridge is used to centralize all event logging to the console as JSON.
 * This is used by the Discord bot to bridge game events to Discord.
 */
public static class EventBridge
{
    private static void Print(string tag, object data)
    {
        if (!MatchConfig.EventBridgeEnabled) return;

        var node = JsonSerializer.SerializeToNode(data);
        if (node is JsonObject jsonObject)
        {
            jsonObject["timestamp"] = DateTime.UtcNow.ToString("O"); // ISO 8601 UTC
            var json = jsonObject.ToJsonString();
            Console.WriteLine($"\n[MATCHUP_{tag.ToUpper()}] {json}\n");
        }
    }

    public static void OnChat(EventPlayerChat @event)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if (player == null || !player.IsValid) return;

        Print("CHAT", new
        {
            player = new
            {
                name = player.PlayerName,
                steamId64 = player.SteamID,
                side = player.TeamNum switch
                {
                    (byte)CsTeam.Terrorist => "T",
                    (byte)CsTeam.CounterTerrorist => "CT",
                    (byte)CsTeam.Spectator => "spectator",
                    _ => "other"
                }
            },
            message = @event.Text,
            isTeamChat = @event.Teamonly
        });
    }

    public static void OnStateChange(GameState oldState, GameState newState)
    {
        Print("STATE_CHANGE", new
        {
            oldState = oldState.ToString().ToLower(),
            newState = newState.ToString().ToLower()
        });
    }

    public static void OnPlayerConnect(EventPlayerConnectFull @event)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return;

        Print("PLAYER_CONNECT", new
        {
            name = player.PlayerName,
            steamId64 = player.SteamID
        });
    }

    public static void OnPlayerDisconnect(EventPlayerDisconnect @event)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return;

        Print("PLAYER_DISCONNECT", new
        {
            name = player.PlayerName,
            steamId64 = player.SteamID
        });
    }

    public static void OnPlayerTeam(EventPlayerTeam @event)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return;

        Print("PLAYER_TEAM", new
        {
            name = player.PlayerName,
            steamId64 = player.SteamID,
            side = @event.Team switch
            {
                (byte)CsTeam.Terrorist => "T",
                (byte)CsTeam.CounterTerrorist => "CT",
                (byte)CsTeam.Spectator => "spectator",
                _ => "other"
            }
        });
    }

    public static void OnReset()
    {
        Print("RESET", new { });
    }

    public static void OnRoundEnd(EventRoundEnd @event)
    {
        Print("ROUND_END", new
        {
            winner = @event.Winner switch
            {
                (byte)CsTeam.Terrorist => "T",
                (byte)CsTeam.CounterTerrorist => "CT",
                _ => "none"
            },
            reason = @event.Reason
        });
    }

    public static void OnMatchEnd(EventCsWinPanelMatch @event, int ctScore, int tScore)
    {
        Print("MATCH_END", new
        {
            ctScore,
            tScore,
            winner = ctScore > tScore ? "CT" : (tScore > ctScore ? "T" : "draw")
        });
    }

    public static void OnPause(string side)
    {
        Print("PAUSE", new { side });
    }

    public static void OnUnpause(string side)
    {
        Print("UNPAUSE", new { side });
    }

    public static void OnBackup(string adminName, int round)
    {
        Print("BACKUP_RESTORE", new
        {
            admin = adminName,
            round
        });
    }
}
