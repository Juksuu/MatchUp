using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MatchUp;

public class SetupState : BaseState
{
    private Dictionary<string, Action<CCSPlayerController, string[]?>> commandActions = new Dictionary<string, Action<CCSPlayerController, string[]?>>();

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Setup state");

        commandActions["!restart"] = (player, args) => OnRestart(player, args);
    }

    public override void Leave() { }

    public override HookResult OnChatCommand(CCSPlayerController player, string command, string[]? args)
    {
        Console.WriteLine("Received a command Setup", command);
        if (commandActions.ContainsKey(command))
        {
            commandActions[command](player, args);
        }
        return HookResult.Changed;
    }

    private void OnRestart(CCSPlayerController player, string[]? args)
    {
        Server.ExecuteCommand("mp_restartgame 3");
    }
}
