using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using MatchUp.states;

namespace MatchUp;

public class MatchUp : BasePlugin
{
    public const string Version = "0.9.0";

    public override string ModuleName => "MatchUp";
    public override string ModuleVersion => Version;

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        PelipajaConfig.Load();
        HttpServer.Start();
        StateMachine.SwitchState(GameState.Loading);

        if (hotReload)
        {
            StateMachine.GetCurrentState().OnMapStart();
        }

        RegisterListener<Listeners.OnMapStart>(_ => StateMachine.GetCurrentState().OnMapStart());
    }

    public override void Unload(bool hotReload)
    {
        HttpServer.Stop();
        base.Unload(hotReload);
    }

    // Console commands
    [ConsoleCommand("matchup_kick_all", "Kick all players")]
    public void OnKickAll(CCSPlayerController? player, CommandInfo command)
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        foreach (var playerEntity in playerEntities)
        {
            if (playerEntity.SteamID == 0) continue; // Player is a bot
            Server.ExecuteCommand($"kick {playerEntity.PlayerName}");
        }
    }

    [ConsoleCommand("matchup_map", "Set match map")]
    public void OnMapSet(CCSPlayerController? player, CommandInfo command)
    {
        command.ReplyToCommand($"setting map with args: {command.GetCommandString}");
        MatchConfig.SetMap(command.GetArg(1));
    }

    [ConsoleCommand("matchup_team_size", "Set match team size")]
    public void OnTeamSizeSet(CCSPlayerController? player, CommandInfo command)
    {
        MatchConfig.SetTeamSize(command.GetArg(1));
    }

    [ConsoleCommand("matchup_knife", "Set match knife round status")]
    public void OnKnifeSet(CCSPlayerController? player, CommandInfo command)
    {
        MatchConfig.SetKnife(command.GetArg(1));
    }

    [ConsoleCommand("matchup_start", "Start match with current config")]
    public void OnMatchStart(CCSPlayerController? player, CommandInfo command)
    {
        MatchConfig.StartMatch();
    }

    [ConsoleCommand("matchup_version", "Prints the current version of MatchUp")]
    public void OnVersion(CCSPlayerController? player, CommandInfo command)
    {
        command.ReplyToCommand($"MatchUp version {ModuleVersion}");
    }

    [ConsoleCommand("matchup_status", "Prints match status as JSON")]
    public void OnMatchStatus(CCSPlayerController? player, CommandInfo command)
    {
        command.ReplyToCommand($"\n{Utils.GetMatchStatusJson() ?? "No match status available"}\n");
    }

    [ConsoleCommand("matchup_demo", "Prints the demo recording and upload status")]
    public void OnDemoStatus(CCSPlayerController? player, CommandInfo command)
    {
        Utils.PrintDemoStatus(command.ReplyToCommand);
    }

    [ConsoleCommand("matchup_reconfigure", "Reloads the MatchUp configs")]
    public void OnReConfigure(CCSPlayerController? player, CommandInfo command)
    {
        // only allow reconfiguring during the setup phase
        if (StateMachine.GetCurrentState().GetType() != typeof(SetupState))
        {
            command.ReplyToCommand("Can only reconfigure during setup phase");
            return;
        }

        MatchConfig.LoadMaps();
        MatchConfig.LoadSettings();
    }

    // Events
    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        // Echo chat messages to console as JSON for Discord bridge
        EventBridge.OnChat(@event);

        if (!@event.Text.StartsWith('.') && !@event.Text.StartsWith('!'))
        {
            return HookResult.Continue;
        }

        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        var result = @event.Text.Split(" ");
        var command = result[0].Trim('!', '.');

        if (command == "reset")
        {
            OnReset();
            return HookResult.Continue;
        }
        if (command == "cancelmatch")
        {
            OnCancelMatch(@event.Userid, player);
            return HookResult.Continue;
        }

        if (command == "confirmcancel")
        {
            OnConfirmCancel(@event.Userid, player);
            return HookResult.Continue;
        }

        var state = StateMachine.GetCurrentState();

        if (result.Length > 1)
        {
            var args = result.Skip(1).ToArray();
            Console.WriteLine($"Got command with args: {command}, {string.Join(", ", args)}");
            state.OnChatCommand(@event.Userid, command, args);

            return HookResult.Continue;
        }

        Console.WriteLine($"Got command: {command}");
        state.OnChatCommand(@event.Userid, command);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        EventBridge.OnPlayerTeam(@event);
        StateMachine.GetCurrentState().OnPlayerTeam(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        EventBridge.OnPlayerConnect(@event);
        StateMachine.GetCurrentState().OnPlayerConnect(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        EventBridge.OnPlayerDisconnect(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnMatchEnd(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        StateMachine.GetCurrentState().OnMatchEnd(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        EventBridge.OnRoundEnd(@event);
        StateMachine.GetCurrentState().OnRoundEnd(@event);
        return HookResult.Continue;
    }

    private static void OnReset()
    {
        EventBridge.OnReset();
        Server.PrintToChatAll($" {ChatColors.Green}Resetting!!!");

        Utils.DelayedCall(TimeSpan.FromSeconds(1), () =>
        {
            StateMachine.SwitchState(GameState.Loading);
            if (!string.IsNullOrEmpty(MatchConfig.Map.WorkshopId))
            {
                Server.ExecuteCommand($"host_workshop_map {MatchConfig.Map.WorkshopId}");
            }
            else
            {
                Server.ExecuteCommand($"changelevel {MatchConfig.Map.Name}");
            }
        });
    }

    private static void OnCancelMatch(int userid, CCSPlayerController player)
    {
        var isOwner = PelipajaConfig.OwnerSteamId != null && player.SteamID.ToString() == PelipajaConfig.OwnerSteamId;
        var isDev = player.SteamID.ToString() == "76561197970226616";

        if (!isOwner && !isDev)
        {
            player.PrintToChat($" {ChatColors.Red}You are not the match owner!");
            return;
        }

        player.PrintToChat($" {ChatColors.Red}Are you sure you want to cancel the match?");
        player.PrintToChat($" {ChatColors.Red}Type !confirmcancel to confirm.");
    }

    private static void OnConfirmCancel(int userid, CCSPlayerController player)
    {
        if (PelipajaConfig.OwnerSteamId == null || player.SteamID.ToString() != PelipajaConfig.OwnerSteamId)
        {
            player.PrintToChat($" {ChatColors.Red}You are not the match owner!");
            return;
        }

        Server.PrintToChatAll($" {ChatColors.Red}Match cancelled by {player.PlayerName}!");
        _ = WebhookClient.PostStatus("cancelled");
    }
}
