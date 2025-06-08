using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class ReadyUpState : BaseState
{
    private bool _tReady;
    private bool _ctReady;

    private readonly List<int> _tPlayersReady = [];
    private readonly List<int> _ctPlayersReady = [];

    public ReadyUpState()
    {
        CommandActions["r"] = (userid, _) => OnPlayerReady(userid);
        CommandActions["ready"] = (userid, _) => OnPlayerReady(userid);
        CommandActions["ur"] = (userid, _) => OnPlayerUnReady(userid);
        CommandActions["unready"] = (userid, _) => OnPlayerUnReady(userid);
        CommandActions["forceready"] = (_, _) => OnForceReady();

        // Used for testing
        CommandActions["bot_ct"] = (userid, _) => OnBotCt(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to ReadyUp state");

        Console.WriteLine("Executing warmup cfg");
        Server.ExecuteCommand("exec MatchUp/warmup.cfg");
    }

    public override void Leave()
    {
        _tReady = false;
        _ctReady = false;

        _tPlayersReady.Clear();
        _ctPlayersReady.Clear();
    }

    public override void OnPlayerTeam(EventPlayerTeam @event)
    {
        var player = @event.Userid;
        if (!@event.Isbot && player != null && player.UserId.HasValue)
        {
            OnPlayerUnReady(player.UserId.Value);
        }
    }

    public override void OnPlayerConnect(EventPlayerConnectFull @event)
    {
        var player = @event.Userid;
        if (player != null)
        {
            player.PrintToChat($" {ChatColors.Green}Please type !ready to ready up!");
        }
    }

    private void OnPlayerReady(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null || !player.IsValid || !player.Pawn.IsValid)
        {
            return;
        }

        switch (player.TeamNum)
        {
            case (byte)CsTeam.Terrorist when !_tPlayersReady.Contains(userid):
                _tPlayersReady.Add(userid);
                break;
            case (byte)CsTeam.CounterTerrorist when !_ctPlayersReady.Contains(userid):
                _ctPlayersReady.Add(userid);
                break;
        }

        _tReady = _tPlayersReady.Count >= MatchConfig.PlayersPerTeam;
        _ctReady = _ctPlayersReady.Count >= MatchConfig.PlayersPerTeam;

        player.PrintToChat($" {ChatColors.Green} You have been marked ready!");

        Server.PrintToChatAll(
            $" {ChatColors.Green}Players ready {ChatColors.DarkRed}{_tPlayersReady.Count + _ctPlayersReady.Count}/{MatchConfig.PlayersPerTeam * 2}");

        if (!_tReady || !_ctReady)
        {
            // not all players are ready yet, nothing to do here
            return;
        }

        Server.PrintToChatAll($" {ChatColors.Green} All players are ready! Starting match!");
        StateMachine.SwitchState(MatchConfig.KnifeRound ? GameState.Knife : GameState.Live);
    }

    private void OnPlayerUnReady(int userid)
    {
        if (_tPlayersReady.Contains(userid))
        {
            _tPlayersReady.Remove(userid);
            _tReady = false;
        }

        if (_ctPlayersReady.Contains(userid))
        {
            _ctPlayersReady.Remove(userid);
            _ctReady = false;
        }

        var player = Utilities.GetPlayerFromUserid(userid);
        if (player != null)
        {
            player.PrintToChat($" {ChatColors.Green} You have been marked unready!");
        }

        Server.PrintToChatAll(
            $" {ChatColors.Green}Players ready {ChatColors.DarkRed}{_tPlayersReady.Count + _ctPlayersReady.Count}/{MatchConfig.PlayersPerTeam * 2}");
    }

    private static void OnForceReady()
    {
        Server.PrintToChatAll($" {ChatColors.Green} Forced ready! Starting match!");
        StateMachine.SwitchState(MatchConfig.KnifeRound ? GameState.Knife : GameState.Live);
    }

    // Used for testing
    private static void OnBotCt(int _)
    {
        Server.ExecuteCommand("bot_add_ct");
    }
}