using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public class LiveState : BaseState
{

    private bool team1Pause = false;
    private bool team2Pause = false;

    public LiveState() : base()
    {
        commandActions["pause"] = (userid, args) => OnPlayerPause(userid);
        commandActions["unpause"] = (userid, args) => OnPlayerUnpause(userid);

        // Used for testing
        commandActions["kill"] = (userid, args) => OnPlayerSuicide(userid);
        commandActions["bot_ct"] = (userid, args) => OnBotCt(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Live state");

        Console.WriteLine("Executing Live cfg");
        Server.ExecuteCommand("exec MatchUp/live.cfg");

        Server.ExecuteCommand("mp_restartgame 3");

        Task.Delay(4000).ContinueWith(t =>
        {
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
        });
    }

    public override void Leave()
    {
        team1Pause = false;
        team2Pause = false;
    }

    private void OnPlayerPause(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        var paused = team1Pause || team2Pause;

        if (paused)
        {
            player.PrintToChat($" {ChatColors.Green} Game is already paused!");
            return;
        }

        team1Pause = true;
        team2Pause = true;

        if (player.TeamNum == (byte)CsTeam.Terrorist)
        {
            Server.PrintToChatAll($" {ChatColors.Green} Terrorists have paused the game!");
        }
        else if (player.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            Server.PrintToChatAll($" {ChatColors.Green} Counter-Terrorists have paused the game!");
        }

        Server.ExecuteCommand("mp_pause_match");
    }

    private void OnPlayerUnpause(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);

        if (player.TeamNum == (byte)CsTeam.Terrorist)
        {
            if (!team1Pause)
            {
                player.PrintToChat($" {ChatColors.Green} Your team already unpaused!");
            }
            else
            {
                team1Pause = false;
                Server.PrintToChatAll($" {ChatColors.Green} Terrorists have unpaused the game!");
            }
        }
        else if (player.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            if (!team2Pause)
            {
                player.PrintToChat($" {ChatColors.Green} Your team already unpaused!");
            }
            else
            {
                team2Pause = false;
                Server.PrintToChatAll($" {ChatColors.Green} Counter-Terrorists have unpaused the game!");
            }
        }

        if (!team1Pause && !team2Pause)
        {
            Server.ExecuteCommand("mp_unpause_match");
        }
    }

    public override HookResult OnMatchEnd(EventCsWinPanelMatch @event)
    {

        var delay = 15;
        var tvEnableCVar = ConVar.Find("tv_enable");
        if (tvEnableCVar != null && tvEnableCVar.GetPrimitiveValue<bool>())
        {
            // TODO: Test which delay should be used here, tv_delay or tv_delay1
            var tvDelayCvar = ConVar.Find("tv_delay");
            if (tvDelayCvar != null)
            {
                delay += tvDelayCvar.GetPrimitiveValue<int>();
            }
        }

        Console.WriteLine($"Waiting for match end panel and cstv delay {delay}");

        Task.Delay(delay * 1000).ContinueWith(t =>
        {
            StateMachine.SwitchState(GameState.Loading);
            Server.ExecuteCommand($"changelevel {Server.MapName}");
        });

        return HookResult.Handled;
    }

    // Used for testing
    private void OnPlayerSuicide(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);

        if (player == null || !player.IsValid || !player.PlayerPawn.IsValid)
            return;

        player.PlayerPawn.Value.CommitSuicide(true, false);
    }

    // Used for testing
    private void OnBotCt(int userid)
    {
        Server.ExecuteCommand("bot_add_ct");
    }
}
