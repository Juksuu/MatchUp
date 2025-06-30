using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class KnifeState : BaseState
{
    private CsTeam _winningTeam;
    private bool _knifeEnded;

    public KnifeState()
    {
        CommandActions["stay"] = (userid, _) => OnStay(userid);
        CommandActions["switch"] = (userid, _) => OnSwitch(userid);

        // Used for testing
        CommandActions["kill"] = (userid, _) => OnPlayerSuicide(userid);
        CommandActions["bot_ct"] = (userid, _) => OnBotCt(userid);
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
        _knifeEnded = false;
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

        _winningTeam = (CsTeam)@event.Winner;

        Server.PrintToChatAll($" {ChatColors.Green}Please select side with !stay/!switch");
        _knifeEnded = true;
    }

    private void OnSwitch(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (!_knifeEnded || player == null || player.TeamNum != (byte)_winningTeam)
        {
            return;
        }

        Server.ExecuteCommand("mp_swapteams");
        StateMachine.SwitchState(GameState.Live);
    }

    private void OnStay(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (_knifeEnded && player != null && player.TeamNum == (byte)_winningTeam)
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