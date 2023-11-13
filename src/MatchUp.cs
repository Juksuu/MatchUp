using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MatchUp;
public class MatchUp : BasePlugin
{
    public override string ModuleName => "MatchUp";
    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        StateMachine.SwitchState(GameState.Loading);

        RegisterEventHandler<EventPlayerChat>((@event, info) =>
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
            var command = result[0];

            var state = StateMachine.getCurrentState();

            if (result.Length > 1)
            {
                var args = result.Skip(1);
                return state.OnChatCommand(player, command, args.ToArray());
            }

            return state.OnChatCommand(player, command);
        });
    }
}
