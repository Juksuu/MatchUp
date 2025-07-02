using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class LiveState : BaseState
{
    private bool _tPause;
    private bool _ctPause;

    private ConVar? _lastBackupConVar;
    private string? _lastRoundPlayedBackupFile;

    public LiveState()
    {
        CommandActions["pause"] = (userid, _) => OnPlayerPause(userid);
        CommandActions["unpause"] = (userid, _) => OnPlayerUnpause(userid);
        CommandActions["backup"] = (userid, _) => OnPlayerBackup(userid);

        // Used for testing
        CommandActions["kill"] = (userid, _) => OnPlayerSuicide(userid);
        CommandActions["bot_ct"] = (userid, _) => OnBotCt(userid);
    }

    public override void Enter(GameState oldState)
    {
        Console.WriteLine("Switched to Live state");
        
        Console.WriteLine("Executing Live cfg");
        Server.ExecuteCommand("exec MatchUp/live.cfg");

        Server.ExecuteCommand("mp_restartgame 3");

        _lastBackupConVar = ConVar.Find("mp_backup_round_file_last");

        Utils.DelayedCall(TimeSpan.FromSeconds(4), () =>
        {
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");
            Server.PrintToChatAll($" {ChatColors.Green}LIVE!");

            CstvManager.StartDemoRecording();
        });
    }

    public override void Leave()
    {
        _tPause = false;
        _ctPause = false;
        _lastRoundPlayedBackupFile = null;
        _lastBackupConVar = null;
    }

    public override void OnMatchEnd(EventCsWinPanelMatch @event)
    {
        var delay = 15;
        delay += CstvManager.GetTvDelay();

        Console.WriteLine($"Waiting for match end panel and cstv delay {delay}");

        Utils.DelayedCall(TimeSpan.FromSeconds(delay), () =>
        {
            CstvManager.StopDemoRecording();

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

        var paused = _tPause || _ctPause;
        if (paused)
        {
            player.PrintToChat($" {ChatColors.Green}Game is already paused!");
            return;
        }

        _tPause = true;
        _ctPause = true;

        switch (player.TeamNum)
        {
            case (byte)CsTeam.Terrorist:
                Server.PrintToChatAll($" {ChatColors.Green}Terrorists have paused the game!");
                break;
            case (byte)CsTeam.CounterTerrorist:
                Server.PrintToChatAll($" {ChatColors.Green}Counter-Terrorists have paused the game!");
                break;
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

        switch (player.TeamNum)
        {
            case (byte)CsTeam.Terrorist when !_tPause:
                player.PrintToChat($" {ChatColors.Green}Your team already unpaused!");
                break;
            case (byte)CsTeam.Terrorist:
                _tPause = false;
                Server.PrintToChatAll($" {ChatColors.Green}Terrorists have unpaused the game!");
                break;
            case (byte)CsTeam.CounterTerrorist when !_ctPause:
                player.PrintToChat($" {ChatColors.Green}Your team already unpaused!");
                break;
            case (byte)CsTeam.CounterTerrorist:
                _ctPause = false;
                Server.PrintToChatAll($" {ChatColors.Green}Counter-Terrorists have unpaused the game!");
                break;
        }
        
        if (!_tPause && !_ctPause)
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

        var paused = _tPause || _ctPause;
        if (!paused)
        {
            player.PrintToChat($" {ChatColors.Red} Game needs to be paused to use backups");
            return;
        }

        var backupFileName = _lastRoundPlayedBackupFile;
        if (_lastBackupConVar != null && !string.IsNullOrEmpty(_lastBackupConVar.StringValue))
        {
            backupFileName = _lastBackupConVar.StringValue;
            _lastRoundPlayedBackupFile = _lastBackupConVar.StringValue;
        }

        if (backupFileName == null)
        {
            player.PrintToChat($" {ChatColors.Red} Could not load backup data");
            return;
        }

        Action<CCSPlayerController, ChatMenuOption> backupHandle = (actionPlayer, option) =>
            OnPlayerSelectBackup(actionPlayer, option.Text);

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

    private void OnPlayerSelectBackup(CCSPlayerController player, string selection)
    {
        if (_lastRoundPlayedBackupFile == null)
        {
            return;
        }

        var round = selection.Split(" ")[1];
        var backupFileName = string.Concat(_lastRoundPlayedBackupFile.SkipLast(6)) + $"{round}.txt";
        player.PrintToChat($"Restoring previous state using backup file {backupFileName}");

        Server.ExecuteCommand($"mp_backup_restore_load_file {backupFileName}");
    }

    // Used for testing
    private static void OnPlayerSuicide(int userid)
    {
        var player = Utilities.GetPlayerFromUserid(userid);

        if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.IsValid)
        {
            return;
        }

        player.PlayerPawn.Value.CommitSuicide(true, false);
    }

    // Used for testing
    private static void OnBotCt(int _)
    {
        Server.ExecuteCommand("bot_add_ct");
    }
}