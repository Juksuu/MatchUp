using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class LiveState : BaseState
{
    private bool team1Pause = false;
    private bool team2Pause = false;

    private ConVar? lastBackupConVar = null;
    private string? lastRoundPlayedBackupFile = null;

    public LiveState() : base()
    {
        commandActions["pause"] = (userid, args) => OnPlayerPause(userid);
        commandActions["unpause"] = (userid, args) => OnPlayerUnpause(userid);
        commandActions["backup"] = (userid, args) => OnPlayerBackup(userid);

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

        lastBackupConVar = ConVar.Find("mp_backup_round_file_last");

        Utils.DelayedCall(TimeSpan.FromSeconds(4), () =>
        {
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");

            CSTVManager.startDemoRecording();
        });
    }

    public override void Leave()
    {
        team1Pause = false;
        team2Pause = false;
        lastRoundPlayedBackupFile = null;
        lastBackupConVar = null;
    }

    public override void OnMatchEnd(EventCsWinPanelMatch @event)
    {
        var delay = 15;
        delay += CSTVManager.getTvDelay();

        Console.WriteLine($"Waiting for match end panel and cstv delay {delay}");

        Utils.DelayedCall(TimeSpan.FromSeconds(delay), () =>
        {
            CSTVManager.stopDemoRecording();

            StateMachine.SwitchState(GameState.Loading);
            Server.ExecuteCommand($"changelevel {Server.MapName}");
        });
    }

    private void OnPlayerPause(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null)
        {
            return;
        }

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
        if (player == null)
        {
            return;
        }

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

    private void OnPlayerBackup(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player == null)
        {
            return;
        }

        var paused = team1Pause || team2Pause;
        if (paused)
        {
            var backupFileName = lastRoundPlayedBackupFile;
            if (lastBackupConVar != null && lastBackupConVar.StringValue != null && lastBackupConVar.StringValue != "")
            {
                backupFileName = lastBackupConVar.StringValue;
                lastRoundPlayedBackupFile = lastBackupConVar.StringValue;
            }

            if (backupFileName != null)
            {
                Action<CCSPlayerController, ChatMenuOption> backupHandle =
                    (CCSPlayerController player, ChatMenuOption option) =>
                        OnPlayerSelectBackup(player, option.Text);

                var backupSelection = new ChatMenu("Backup selection");

                var lastRound = int.Parse(string.Concat(backupFileName.SkipLast(4).TakeLast(2)));
                player.PrintToChat($"Last backup file {lastRound}");

                for (var i = 0; i <= lastRound; i++)
                {
                    var roundNumberString = i < 10 ? $"0{i}" : $"{i}";
                    backupSelection.AddMenuOption($"round {roundNumberString}", backupHandle);
                }

                MenuManager.OpenChatMenu(player, backupSelection);
            }
            else
            {
                player.PrintToChat($" {ChatColors.Red} Could not load backup data");
            }
        }
        else
        {
            player.PrintToChat($" {ChatColors.Red} Game needs to be paused to use backups");
        }
    }

    private void OnPlayerSelectBackup(CCSPlayerController player, string selection)
    {
        if (lastRoundPlayedBackupFile != null)
        {
            var round = selection.Split(" ")[1];
            var backupFileName = string.Concat(lastRoundPlayedBackupFile.SkipLast(6)) + $"{round}.txt";
            player.PrintToChat($"Restoring previous state using backup file {backupFileName}");

            Server.ExecuteCommand($"mp_backup_restore_load_file {backupFileName}");
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