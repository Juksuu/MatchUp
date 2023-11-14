using CounterStrikeSharp.API;

namespace MatchUp;

public class LoadingState : BaseState
{
    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Loading state");
    }

    public override void Leave() { }

    public override void OnMapStart()
    {
        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");

        Task.Delay(1000).ContinueWith(t =>
        {
            StateMachine.SwitchState(GameState.Setup);
        });
    }
}
