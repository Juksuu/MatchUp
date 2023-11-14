using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace MatchUp;
public class MatchUp : BasePlugin
{
    public override string ModuleName => "MatchUp";
    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        StateMachine.SwitchState(GameState.Loading);

        if (hotReload)
        {
            StateMachine.getCurrentState().OnMapStart();
        }

        RegisterListener<Listeners.OnMapStart>(name => StateMachine.getCurrentState().OnMapStart());
    }

    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if (!@event.Text.StartsWith(".") && !@event.Text.StartsWith("!"))
        {
            return HookResult.Continue;
        }

        CCSPlayerController player = Utilities.GetPlayerFromUserid(@event.Userid);
        if (!player.IsValid)
        {
            return HookResult.Continue;
        }

        var result = @event.Text.Split(" ");
        var command = result[0].Trim(new Char[] { '!', '.' });

        var state = StateMachine.getCurrentState();

        if (result.Length > 1)
        {
            var args = result.Skip(1).ToArray();
            Console.WriteLine($"Got command with args: {command}, {string.Join(", ", args)}");
            return state.OnChatCommand(@event.Userid, command!, args);
        }

        Console.WriteLine($"Got command: {command}");
        return state.OnChatCommand(@event.Userid, command!);
    }

    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        var state = StateMachine.getCurrentState();
        return state.OnPlayerTeam(@event);
    }
}
