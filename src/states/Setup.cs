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
        commandActions["knife"] = (userid, args) => OnKnife(userid, args);
        commandActions["config"] = (userid, option) => MatchConfig.print(userid);
        commandActions["start"] = (userid, option) => MatchConfig.StartMatch();
        commandActions["help"] = (userid, option) => OnHelp(userid);
    }


    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Setup state");
    }

    public override void Leave() { }

    private void OnMatchMapChange(CCSPlayerController player, string selection)
    {
        MatchConfig.setMap(selection, player);
    }

    private void OnHelp(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        player.PrintToChat($" {ChatColors.Yellow}Commands:");
        player.PrintToChat($" {ChatColors.Green}!map {ChatColors.Default} select map for match");
        player.PrintToChat($" {ChatColors.Green}!start {ChatColors.Default} start match with current config");
        player.PrintToChat($" {ChatColors.Green}!config {ChatColors.Default} print current match config");
        player.PrintToChat($" {ChatColors.Green}!team_size <number> {ChatColors.Default} set team size for match");
        player.PrintToChat($" {ChatColors.Green}!knife <boolean> {ChatColors.Default} set knife round for match");
    }

    private void OnTeamSize(int userid, string[]? args)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (args == null || !MatchConfig.setTeamSize(args[0], player))
        {
            player.PrintToChat("Command usage: !team_size <number>");
        }
    }

    private void OnKnife(int userid, string[]? args)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (args == null || !MatchConfig.setKnife(args[0], player))
        {
            player.PrintToChat("Command usage: !knife <boolean>");
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
