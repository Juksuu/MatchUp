using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;
public class MatchUp : BasePlugin
{
    public override string ModuleName => "MatchUp";
    public override string ModuleVersion => "0.6.0";

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        StateMachine.SwitchState(GameState.Loading);

        if (hotReload)
        {
            StateMachine.getCurrentState().OnMapStart();
        }

        RegisterListener<Listeners.OnMapStart>(name => StateMachine.getCurrentState().OnMapStart());
    }

    // Console commands
    [ConsoleCommand("matchup_kick_all", "Kick all players")]
    public void OnKickAll(CCSPlayerController? player, CommandInfo command)
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        foreach (var entity in playerEntities)
        {
            if (player == null) continue;
            if (player.SteamID == 0) continue; // Player is a bot

            Server.ExecuteCommand($"kick {player.PlayerName}");
        }
    }

    [ConsoleCommand("matchup_map", "Set match map")]
    public void OnMapSet(CCSPlayerController? player, CommandInfo command)
    {
        Console.WriteLine($"setting map with args: {command.GetCommandString}");
        MatchConfig.setMap(command.GetArg(1));
    }

    [ConsoleCommand("matchup_team_size", "Set match team size")]
    public void OnTeamSizeSet(CCSPlayerController? player, CommandInfo command)
    {
        MatchConfig.setTeamSize(command.GetArg(1));
    }

    [ConsoleCommand("matchup_knife", "Set match knife round status")]
    public void OnKnifeSet(CCSPlayerController? player, CommandInfo command)
    {
        MatchConfig.setKnife(command.GetArg(1));
    }

    [ConsoleCommand("matchup_start", "Start match with current config")]
    public void OnMatchStart(CCSPlayerController? player, CommandInfo command)
    {
        MatchConfig.StartMatch();
    }


    // Events
    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if (!@event.Text.StartsWith(".") && !@event.Text.StartsWith("!"))
        {
            return HookResult.Continue;
        }

        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        var result = @event.Text.Split(" ");
        var command = result[0].Trim(new Char[] { '!', '.' });

        if (command == "reset")
        {
            OnReset();
            return HookResult.Continue;
        }

        var state = StateMachine.getCurrentState();

        if (result.Length > 1)
        {
            var args = result.Skip(1).ToArray();
            Console.WriteLine($"Got command with args: {command}, {string.Join(", ", args)}");
            state.OnChatCommand(@event.Userid, command!, args);

            return HookResult.Continue;
        }

        Console.WriteLine($"Got command: {command}");
        state.OnChatCommand(@event.Userid, command!);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        StateMachine.getCurrentState().OnPlayerTeam(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        StateMachine.getCurrentState().OnPlayerConnect(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnMatchEnd(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        StateMachine.getCurrentState().OnMatchEnd(@event);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        StateMachine.getCurrentState().OnRoundEnd(@event);
        return HookResult.Continue;
    }

    private void OnReset()
    {
        Server.PrintToChatAll($" {ChatColors.Green}Resetting!!!");

        Utils.DelayedCall(TimeSpan.FromSeconds(1), () =>
        {
            StateMachine.SwitchState(GameState.Loading);
            Server.ExecuteCommand($"changelevel {Server.MapName}");
        });
    }
}
