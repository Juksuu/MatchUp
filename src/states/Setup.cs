using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public class SetupState : BaseState
{

    private ChatMenu mapSelection = new ChatMenu("Map Selection");

    public SetupState() : base()
    {
        var mapChangeHandle = (CCSPlayerController player, ChatMenuOption option) => OnMatchMapChange(player, option.Text);
        foreach (string map in MatchConfig.mapPool)
        {
            mapSelection.AddMenuOption(map, mapChangeHandle);
        }

        commandActions["map"] = (userid, args) => ChatMenus.OpenMenu(Utilities.GetPlayerFromUserid(userid), mapSelection);
        commandActions["team_size"] = (userid, args) => OnTeamSize(userid, args);
        commandActions["config"] = (userid, option) => MatchConfig.print(userid);
        commandActions["start"] = (userid, option) => OnMatchStart();
        commandActions["help"] = (userid, option) => OnHelp(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Setup state");
    }

    public override void Leave() { }

    private void OnMatchMapChange(CCSPlayerController player, string selection)
    {
        player.PrintToChat($"Setting map to: {ChatColors.Gold} {selection}");
        MatchConfig.map = selection;
    }

    private void OnHelp(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        player.PrintToChat($" {ChatColors.Yellow}Commands:");
        player.PrintToChat($" {ChatColors.Green}!map {ChatColors.Default} select map for match");
        player.PrintToChat($" {ChatColors.Green}!start {ChatColors.Default} start match with current config");
        player.PrintToChat($" {ChatColors.Green}!config {ChatColors.Default} print current match config");
        player.PrintToChat($" {ChatColors.Green}!team_size {ChatColors.Default} set team size for match");
    }

    private void OnTeamSize(int userid, string[]? args)
    {
        var player = Utilities.GetPlayerFromUserid(userid);

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

    private void OnMatchStart()
    {
        Server.PrintToChatAll($" {ChatColors.Green}Setting up match with current config");
        MatchConfig.print();
        if (Server.MapName == MatchConfig.map)
        {
            Server.ExecuteCommand("mp_restartgame 1");
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

    public override HookResult OnPlayerTeam(EventPlayerTeam @event)
    {
        return HookResult.Continue;
    }

    public override HookResult OnMatchEnd(EventCsWinPanelMatch @event)
    {
        return HookResult.Continue;
    }
}
