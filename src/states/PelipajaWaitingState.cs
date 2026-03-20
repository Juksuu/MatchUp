using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp.states;

public class PelipajaWaitingState : BaseState
{
    public override void Enter(GameState oldState)
    {
        Console.WriteLine("[Pelipaja] Waiting for config from Next.js...");
    }

    public override void Leave() { }

    public override void OnPlayerConnect(EventPlayerConnectFull @event)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return;

        var steamId = player.SteamID.ToString();

        if (PelipajaConfig.Team1?.Players.Contains(steamId) == true)
        {
            player.ChangeTeam(CsTeam.Terrorist);
            player.PrintToChat($" {ChatColors.Green}You have been assigned to {PelipajaConfig.Team1.Name}");
        }
        else if (PelipajaConfig.Team2?.Players.Contains(steamId) == true)
        {
            player.ChangeTeam(CsTeam.CounterTerrorist);
            player.PrintToChat($" {ChatColors.Green}You have been assigned to {PelipajaConfig.Team2.Name}");
        }
    }
}