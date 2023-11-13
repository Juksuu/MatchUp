using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MatchUp;

public class LoadingState : BaseState
{
    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Loading state");

        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");

        Task.Delay(1000).ContinueWith(t =>
        {
            StateMachine.SwitchState(GameState.Setup);
        });

    }

    public override void Leave() { }

    public override HookResult OnChatCommand(CCSPlayerController player, string command, string[]? args)
    {
        return HookResult.Continue;
    }
}
