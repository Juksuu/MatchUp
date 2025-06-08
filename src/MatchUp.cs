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
    public override string ModuleName => "MatchUp";
    public override string ModuleVersion => "0.6.1";

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        StateMachine.SwitchState(GameState.Loading);

        if (hotReload)
        {
            StateMachine.GetCurrentState().OnMapStart();
        }

        RegisterListener<Listeners.OnMapStart>(_ => StateMachine.GetCurrentState().OnMapStart());
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
        Console.WriteLine($"setting map with args: {command.GetCommandString}");
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

    [ConsoleCommand("matchup_reconfigure", "Reloads the MatchUp configs")]
    public void OnReConfigure(CCSPlayerController? player, CommandInfo command)
    {
        // only allow reconfiguring during the setup phase
        if (StateMachine.GetCurrentState().GetType() != typeof(SetupState))
        {
            Console.WriteLine("Can only reconfigure during setup phase");
            return;
        }

        MatchConfig.LoadMaps();
        MatchConfig.LoadSettings();
    }

    // Events
    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
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
        StateMachine.GetCurrentState().OnPlayerTeam(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        StateMachine.GetCurrentState().OnPlayerConnect(@event);
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
        StateMachine.GetCurrentState().OnRoundEnd(@event);
        return HookResult.Continue;
    }

    private static void OnReset()
    {
        Server.PrintToChatAll($" {ChatColors.Green}Resetting!!!");

        Utils.DelayedCall(TimeSpan.FromSeconds(1), () =>
        {
            StateMachine.SwitchState(GameState.Loading);
            Server.ExecuteCommand($"changelevel {Server.MapName}");
        });
    }
}