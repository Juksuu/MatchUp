using CounterStrikeSharp.API.Core;
using MatchUp.states;

namespace MatchUp;

public enum GameState
{
    Loading,
    Setup,
    ReadyUp,
    Knife,
    Live,
    End
}

public abstract class BaseState
{
    protected readonly Dictionary<string, Action<int, string[]?>> CommandActions = new();

    public abstract void Enter(GameState oldState);
    public abstract void Leave();

    public virtual void OnMapStart() { }

    public virtual void OnPlayerTeam(EventPlayerTeam @event) { }

    public virtual void OnPlayerConnect(EventPlayerConnectFull @event) { }

    public virtual void OnMatchEnd(EventCsWinPanelMatch @event) { }

    public virtual void OnRoundEnd(EventRoundEnd @event) { }

    public virtual void OnChatCommand(int userid, string command, string[]? args = null)
    {
        if (CommandActions.TryGetValue(command, out var action))
        {
            action(userid, args);
        }
    }
}

public static class StateMachine
{
    private static GameState _currentGameState;
    private static readonly Dictionary<GameState, BaseState> GameStates = new()
    {
        { GameState.Loading, new LoadingState() },
        { GameState.Setup, new SetupState() },
        { GameState.ReadyUp, new ReadyUpState() },
        { GameState.Live, new LiveState() },
        { GameState.Knife, new KnifeState() },
    };

    public static void SwitchState(GameState state)
    {
        GameStates[_currentGameState].Leave();
        GameStates[state].Enter(_currentGameState);

        _currentGameState = state;
    }

    public static BaseState GetCurrentState()
    {
        return GameStates[_currentGameState];
    }
}