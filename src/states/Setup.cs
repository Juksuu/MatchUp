using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public class SetupState : BaseState
{
    private Dictionary<string, Action<CCSPlayerController, string[]?>> commandActions = new Dictionary<string, Action<CCSPlayerController, string[]?>>();

    private ChatMenu mapSelection = new ChatMenu("Map Selection");

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Setup state");


        var mapChangeHandle = (CCSPlayerController player, ChatMenuOption option) => OnMatchMapChange(player, option.Text);
        foreach (string map in MatchConfig.mapPool)
        {
            mapSelection.AddMenuOption(map, mapChangeHandle);
        }

        commandActions["!map"] = (player, args) => ChatMenus.OpenMenu(player, mapSelection);
        commandActions["!team_size"] = (player, args) => OnTeamSize(player, args);
        commandActions["!config"] = (player, option) => MatchConfig.printToPlayer(player);
        commandActions["!start"] = (player, option) => OnMatchStart(player);
        commandActions["!help"] = (player, option) => OnHelp(player);
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

    private void OnMatchMapChange(CCSPlayerController player, string selection)
    {
        player.PrintToChat($"Setting map to: {ChatColors.Gold} {selection}");
        MatchConfig.map = selection;
    }

    private void OnHelp(CCSPlayerController player)
    {
        player.PrintToChat($@"Options can be changed with
                {ChatColors.Green}!map {ChatColors.Default} and {ChatColors.Green} !team_size <number>");
    }

    private void OnTeamSize(CCSPlayerController player, string[]? args)
    {
        var result = 0;
        if (args != null && Int32.TryParse(args[0], out result))
        {
            player.PrintToChat($"Setting team size to: {result}");
            MatchConfig.playersPerTeam = result;
        }
        else
        {
            player.PrintToChat("Command usage: !team_size <number>");
        }
    }

    private void OnMatchStart(CCSPlayerController player)
    {
        if (Server.MapName == MatchConfig.map)
        {
            StateMachine.SwitchState(GameState.Readyup);
        }
        else
        {
            Server.ExecuteCommand($"changelevel {MatchConfig.map}");
        }
    }

    public override void OnMapStart()
    {
        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");

        Task.Delay(1000).ContinueWith(t =>
        {
            StateMachine.SwitchState(GameState.Readyup);
        });
    }
}
