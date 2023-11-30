using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public class ReadyUpState : BaseState
{
    private bool team1Ready = false;
    private bool team2Ready = false;

    private List<int> team1PlayersReady = new List<int>();
    private List<int> team2PlayersReady = new List<int>();

    public ReadyUpState() : base()
    {
        commandActions["r"] = (userid, args) => OnPlayerReady(userid);
        commandActions["ready"] = (userid, args) => OnPlayerReady(userid);
        commandActions["ur"] = (userid, args) => OnPlayerUnReady(userid);
        commandActions["unready"] = (userid, args) => OnPlayerUnReady(userid);
        commandActions["forceready"] = (userid, args) => OnForceReady();

        // Used for testing
        commandActions["bot_ct"] = (userid, args) => OnBotCt(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to ReadyUp state");

        Server.PrintToChatAll($" {ChatColors.Green}Please type !ready to ready up!");
    }

    public override void Leave()
    {
        team1Ready = false;
        team2Ready = false;

        team1PlayersReady.Clear();
        team2PlayersReady.Clear();
    }

    public override HookResult OnPlayerTeam(EventPlayerTeam @event)
    {
        var player = @event.Userid;

        if (!@event.Isbot && player.UserId.HasValue)
        {
            OnPlayerUnReady(player.UserId.Value);
        }

        return HookResult.Continue;
    }

    public override HookResult OnPlayerConnect(EventPlayerConnectFull @event)
    {
        var player = @event.Userid;
        player.PrintToChat($" {ChatColors.Green}Please type !ready to ready up!");

        return HookResult.Continue;
    }

    private void OnPlayerReady(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (!player.IsValid || !player.Pawn.IsValid)
        {
            return;
        }

        if (player.TeamNum == (byte)CsTeam.Terrorist)
        {
            if (!team1PlayersReady.Contains(userid))
            {
                team1PlayersReady.Add(userid);
            }
        }
        else if (player.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            if (!team2PlayersReady.Contains(userid))
            {
                team2PlayersReady.Add(userid);
            }
        }

        team1Ready = team1PlayersReady.Count >= MatchConfig.playersPerTeam;
        team2Ready = team2PlayersReady.Count >= MatchConfig.playersPerTeam;

        player.PrintToChat($" {ChatColors.Green} You have been marked ready!");

        Server.PrintToChatAll($@" {ChatColors.Green}Players ready
                {ChatColors.Darkred}{team1PlayersReady.Count + team2PlayersReady.Count}/{MatchConfig.playersPerTeam * 2}");


        if (team1Ready && team2Ready)
        {
            Server.PrintToChatAll($" {ChatColors.Green} All players are ready! Starting match!");
            if (MatchConfig.knifeRound)
            {
                StateMachine.SwitchState(GameState.Knife);
            }
            else
            {
                StateMachine.SwitchState(GameState.Live);
            }
        }
    }

    private void OnPlayerUnReady(int userid)
    {
        if (team1PlayersReady.Contains(userid))
        {
            team1PlayersReady.Remove(userid);
            team1Ready = false;
        }

        if (team2PlayersReady.Contains(userid))
        {
            team2PlayersReady.Remove(userid);
            team2Ready = false;
        }

        var player = Utilities.GetPlayerFromUserid(userid);
        player.PrintToChat($" {ChatColors.Green} You have been marked unready!");

        Server.PrintToChatAll($@" {ChatColors.Green}Players ready
                {ChatColors.Darkred}{team1PlayersReady.Count + team2PlayersReady.Count}/{MatchConfig.playersPerTeam * 2}");
    }

    private void OnForceReady()
    {
        Server.PrintToChatAll($" {ChatColors.Green} ForceStart! Starting match!");
        if (MatchConfig.knifeRound)
        {
            StateMachine.SwitchState(GameState.Knife);
        }
        else
        {
            StateMachine.SwitchState(GameState.Live);
        }
    }

    // Used for testing
    private void OnBotCt(int userid)
    {
        Server.ExecuteCommand("bot_add_ct");
    }
}
