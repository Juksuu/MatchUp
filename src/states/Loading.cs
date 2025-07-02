namespace MatchUp.states;

public class LoadingState : BaseState
{
    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Loading state");

        MatchConfig.LoadMaps();
        MatchConfig.LoadSettings();
    }

    public override void Leave() { }

    public override void OnMapStart()
    {
        Utils.DelayedCall(TimeSpan.FromSeconds(1), () => { StateMachine.SwitchState(GameState.Setup); });
    }
}