using CounterStrikeSharp.API.Core;

namespace MatchUp;

public enum GameState
{
    Loading,
    Setup,
    Readyup,
    Knife,
    Live,
    End
}

public abstract class BaseState
{
    public abstract void Enter(GameState oldState);
    public abstract void Leave();

    public abstract void OnMapStart();
    public abstract HookResult OnChatCommand(CCSPlayerController player, string command, string[]? args = null);
}

public static class StateMachine
{
    private static GameState currentGameState;
    private static Dictionary<GameState, BaseState> gameStates = new Dictionary<GameState, BaseState>() {
        { GameState.Loading, new LoadingState() },
        { GameState.Setup, new SetupState() },
        { GameState.Readyup, new ReadyUpState() }
    };

    public static void SwitchState(GameState state)
    {
        gameStates[currentGameState].Leave();
        gameStates[state].Enter(currentGameState);

        currentGameState = state;
    }

    public static BaseState getCurrentState()
    {
        return gameStates[currentGameState];
    }
}
