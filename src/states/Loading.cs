namespace MatchUp;

public class LoadingState : BaseState
{
    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Loading state");

        MatchConfig.loadMaps();
        MatchConfig.loadSettings();
    }

    public override void Leave() { }

    public override void OnMapStart()
    {
        Utils.DelayedCall(TimeSpan.FromSeconds(1), () => { StateMachine.SwitchState(GameState.Setup); });
    }
}