using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class SetupState : BaseState
{
    public SetupState() : base()
    {
        CommandActions["map"] = (userid, args) => OnMapSelection(userid);
        CommandActions["team_size"] = (userid, args) => OnTeamSize(userid, args);
        CommandActions["knife"] = (userid, args) => OnKnife(userid, args);
        CommandActions["config"] = (userid, option) => MatchConfig.print(userid);
        CommandActions["start"] = (userid, option) => MatchConfig.StartMatch();
        CommandActions["help"] = (userid, option) => OnHelp(userid);
    }


    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Setup state");

        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");
    }

    public override void Leave() { }

    private void OnMapSelection(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null)
        {
            return;
        }

        Action<CCSPlayerController, ChatMenuOption> mapChangeHandle =
            (CCSPlayerController player, ChatMenuOption option) => MatchConfig.setMap(option.Text, player);

        var mapSelection = new ChatMenu("Map Selection");

        foreach (string map in MatchConfig.mapPool)
        {
            mapSelection.AddMenuOption(map, mapChangeHandle);
        }

        MenuManager.OpenChatMenu(player, mapSelection);
    }

    private void OnHelp(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null)
        {
            return;
        }

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
        if (player != null && (args == null || !MatchConfig.setTeamSize(args[0], player)))
        {
            player.PrintToChat("Command usage: !team_size <number>");
        }
    }

    private void OnKnife(int userid, string[]? args)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player != null && (args == null || !MatchConfig.setKnife(args[0], player)))
        {
            player.PrintToChat("Command usage: !knife <boolean>");
        }
    }

    public override void OnMapStart()
    {
        Utils.DelayedCall(TimeSpan.FromSeconds(1), () => { StateMachine.SwitchState(GameState.ReadyUp); });
    }
}