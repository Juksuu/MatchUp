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
    protected Dictionary<string, Action<int, string[]?>> commandActions = new Dictionary<string, Action<int, string[]?>>();

    public abstract void Enter(GameState oldState);
    public abstract void Leave();

    public virtual void OnMapStart() { }

    public virtual HookResult OnPlayerTeam(EventPlayerTeam @event) { return HookResult.Continue; }
    public virtual HookResult OnMatchEnd(EventCsWinPanelMatch @event) { return HookResult.Continue; }
    public virtual HookResult OnPlayerConnect(EventPlayerConnectFull @event) { return HookResult.Continue; }

    public virtual HookResult OnChatCommand(int userid, string command, string[]? args = null)
    {
        if (commandActions.ContainsKey(command))
        {
            commandActions[command](userid, args);
        }
        return HookResult.Changed;
    }

}

public static class StateMachine
{
    private static GameState currentGameState;
    private static Dictionary<GameState, BaseState> gameStates = new Dictionary<GameState, BaseState>() {
        { GameState.Loading, new LoadingState() },
        { GameState.Setup, new SetupState() },
        { GameState.Readyup, new ReadyUpState() },
        { GameState.Live, new LiveState() }
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
