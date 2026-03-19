namespace MatchUp.states;

public class PelipajaWaitingState : BaseState
{
    public override void Enter(GameState oldState)
    {
        Console.WriteLine("[Pelipaja] Waiting for config from Next.js...");
    }

    public override void Leave()
    {
        
    }
}