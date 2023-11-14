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
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to ReadyUp state");
    }

    public override void Leave()
    {
        team1Ready = false;
        team2Ready = false;
    }

    public override void OnMapStart() { }

    public override HookResult OnPlayerTeam(EventPlayerTeam @event)
    {
        var player = @event.Userid;

        if (!@event.Isbot && player.UserId.HasValue)
        {
            OnPlayerUnReady(player.UserId.Value);
        }

        return HookResult.Continue;
    }

    private void OnPlayerReady(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (!player.IsValid || !player.Pawn.IsValid)
        {
            return;
        }

        if (player.TeamNum == 2)
        {
            team1PlayersReady.Add(userid);
        }
        else if (player.TeamNum == 3)
        {
            team2PlayersReady.Add(userid);
        }

        team1Ready = team1PlayersReady.Count >= MatchConfig.playersPerTeam;
        team2Ready = team2PlayersReady.Count >= MatchConfig.playersPerTeam;

        player.PrintToChat($" {ChatColors.Green} You have been marked ready!");

        Server.PrintToChatAll($@" {ChatColors.Green}Players ready
                {ChatColors.Darkred}{team1PlayersReady.Count + team2PlayersReady.Count}/{MatchConfig.playersPerTeam * 2}");


        if (team1Ready && team2Ready)
        {
            Server.PrintToChatAll($" {ChatColors.Green} All players are ready! Starting match!");
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
}
