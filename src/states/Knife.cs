using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class KnifeState : BaseState
{
    private CsTeam winningTeam;
    private bool knifeEnded;

    public KnifeState()
    {
        commandActions["stay"] = (userid, _) => OnStay(userid);
        commandActions["switch"] = (userid, _) => OnSwitch(userid);

        // Used for testing
        commandActions["kill"] = (userid, _) => OnPlayerSuicide(userid);
        commandActions["bot_ct"] = (userid, _) => OnBotCt(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Knife state");

        Console.WriteLine("Executing Live cfg");
        Server.ExecuteCommand("exec MatchUp/knife.cfg");

        Server.ExecuteCommand("mp_restartgame 3");

        Utils.DelayedCall(TimeSpan.FromSeconds(4), () =>
        {
            Server.PrintToChatAll($" {ChatColors.Green}KNIFE!");
            Server.PrintToChatAll($" {ChatColors.Green}KNIFE!");
            Server.PrintToChatAll($" {ChatColors.Green}KNIFE!");
        });
    }

    public override void Leave()
    {
        knifeEnded = false;
    }

    public override void OnRoundEnd(EventRoundEnd @event)
    {
        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");

        switch (@event.Winner)
        {
            case (byte)CsTeam.Terrorist:
                Server.PrintToChatAll($" {ChatColors.Green}Knife round ended: Terrorists win");
                break;
            case (byte)CsTeam.CounterTerrorist:
                Server.PrintToChatAll($" {ChatColors.Green}Knife round ended: Counter-Terrorists win");
                break;
        }

        winningTeam = (CsTeam)@event.Winner;

        Server.PrintToChatAll($" {ChatColors.Green}Please select side with !stay/!switch");
        knifeEnded = true;
    }

    private void OnSwitch(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (!knifeEnded || player == null || player.TeamNum != (byte)winningTeam)
        {
            return;
        }

        Server.ExecuteCommand("mp_swapteams");
        StateMachine.SwitchState(GameState.Live);
    }

    private void OnStay(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (knifeEnded && player != null && player.TeamNum == (byte)winningTeam)
        {
            StateMachine.SwitchState(GameState.Live);
        }
    }


    // Used for testing
    private static void OnPlayerSuicide(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);

        if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.IsValid)
            return;

        player.PlayerPawn.Value.CommitSuicide(true, false);
    }

    // Used for testing
    private static void OnBotCt(int _)
    {
        Server.ExecuteCommand("bot_add_ct");
    }
}