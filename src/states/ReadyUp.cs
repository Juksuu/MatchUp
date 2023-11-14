using CounterStrikeSharp.API.Core;

namespace MatchUp;

public class ReadyUpState : BaseState
{

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to ReadyUp state");
    }

    public override void Leave() { }

    public override HookResult OnChatCommand(CCSPlayerController player, string command, string[]? args)
    {
        return HookResult.Continue;
    }

    public override void OnMapStart()
    {
        throw new NotImplementedException();
    }
}
