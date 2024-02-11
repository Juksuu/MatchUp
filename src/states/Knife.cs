using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public class KnifeState : BaseState
{

    private CsTeam winningTeam;
    private bool knifeEnded = false;

    private int restartsRequired = 1;
    private int restartsRemaining = 1;
    private bool waitForRestarts = false;

    public KnifeState() : base()
    {
        commandActions["stay"] = (userid, args) => OnStay(userid);
        commandActions["switch"] = (userid, args) => OnSwitch(userid);

        // Used for testing
        commandActions["kill"] = (userid, args) => OnPlayerSuicide(userid);
        commandActions["bot_ct"] = (userid, args) => OnBotCt(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Knife state");

        Console.WriteLine("Executing Live cfg");
        Server.ExecuteCommand("exec MatchUp/knife.cfg");

        // Wait for restarts
        // Number of restarts is restartsRequired + 1 since ending warmup counts as restart
        waitForRestarts = true;
        restartsRemaining = restartsRequired + 1;

        Server.ExecuteCommand("mp_restartgame 3");
    }

    public override void Leave()
    {
        knifeEnded = false;
    }

    public override void OnRoundEnd(EventRoundEnd @event)
    {
        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");

        if (@event.Winner == (byte)CsTeam.Terrorist)
        {
            Server.PrintToChatAll($" {ChatColors.Green}Kniferound ended: Terrorists win");
        }
        else if (@event.Winner == (byte)CsTeam.CounterTerrorist)
        {
            Server.PrintToChatAll($" {ChatColors.Green}Kniferound ended: Counter-Terrorists win");
        }

        winningTeam = (CsTeam)@event.Winner;

        Server.PrintToChatAll($" {ChatColors.Green}Please select side with !stay/!switch");
        knifeEnded = true;
    }

    public override void OnBeginNewMatch(EventBeginNewMatch @event)
    {
        if (waitForRestarts)
        {
            restartsRemaining--;

            if (restartsRemaining == 0)
            {
                Server.PrintToChatAll($" {ChatColors.Green}KNIFE!");
                Server.PrintToChatAll($" {ChatColors.Green}KNIFE!");
                Server.PrintToChatAll($" {ChatColors.Green}KNIFE!");

                waitForRestarts = false;
            }
        }
    }

    private void OnSwitch(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (knifeEnded && player.TeamNum == (byte)winningTeam)
        {
            Server.ExecuteCommand("mp_swapteams");
            StateMachine.SwitchState(GameState.Live);
        }
    }

    private void OnStay(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (knifeEnded && player.TeamNum == (byte)winningTeam)
        {
            StateMachine.SwitchState(GameState.Live);
        }
    }


    // Used for testing
    private void OnPlayerSuicide(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);

        if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.IsValid)
            return;

        player.PlayerPawn.Value.CommitSuicide(true, false);
    }

    // Used for testing
    private void OnBotCt(int userid)
    {
        Server.ExecuteCommand("bot_add_ct");
    }
}
