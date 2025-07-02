using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class SetupState : BaseState
{
    public SetupState()
    {
        CommandActions["map"] = (userid, _) => OnMapSelection(userid);
        CommandActions["team_size"] = OnTeamSize;
        CommandActions["knife"] = OnKnife;
        CommandActions["config"] = (userid, _) => MatchConfig.Print(userid);
        CommandActions["start"] = (_, _) => MatchConfig.StartMatch();
        CommandActions["help"] = (userid, _) => OnHelp(userid);
    }


    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Setup state");

        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");
    }

    public override void Leave() { }

    public override void OnMapStart()
    {
        Utils.DelayedCall(TimeSpan.FromSeconds(1), () => { StateMachine.SwitchState(GameState.ReadyUp); });
    }

    private static void OnMapSelection(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null)
        {
            return;
        }

        Action<CCSPlayerController, ChatMenuOption> mapChangeHandle =
            (actionPlayer, option) => MatchConfig.SetMap(option.Text, actionPlayer);

        var mapSelection = new ChatMenu("Map Selection");

        foreach (var map in MatchConfig.MapPool)
        {
            mapSelection.AddMenuOption(map, mapChangeHandle);
        }

        MenuManager.OpenChatMenu(player, mapSelection);
    }

    private static void OnHelp(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null)
        {
            return;
        }

        player.PrintToChat($" {ChatColors.Yellow}Commands:");
        player.PrintToChat($" {ChatColors.Green}!map {ChatColors.Default}select map for match");
        player.PrintToChat($" {ChatColors.Green}!start {ChatColors.Default}start match with current config");
        player.PrintToChat($" {ChatColors.Green}!config {ChatColors.Default}print current match config");
        player.PrintToChat($" {ChatColors.Green}!team_size <number> {ChatColors.Default}set team size for match");
        player.PrintToChat($" {ChatColors.Green}!knife <boolean> {ChatColors.Default}set knife round for match");
    }

    private static void OnTeamSize(int userid, string[]? args)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player != null && (args == null || !MatchConfig.SetTeamSize(args[0], player)))
        {
            player.PrintToChat("Command usage: !team_size <number>");
        }
    }

    private static void OnKnife(int userid, string[]? args)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player != null && (args == null || !MatchConfig.SetKnife(args[0], player)))
        {
            player.PrintToChat("Command usage: !knife <boolean>");
        }
    }
}