using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchUp;

public static class Utils
{
    public static async void DelayedCall(TimeSpan delay, Action callback)
    {
        try
        {
            await Task.Delay(delay);
            await Server.NextWorldUpdateAsync(callback);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while executing delayed call: {e.Message}");
        }
    }

    public static bool ParseBoolEnv(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrEmpty(value)) return false;

        return value.Trim() switch
        {
            "1" => true,
            "0" => false,
            _ => bool.TryParse(value, out var result) && result
        };
    }

    public static void PrintDemoStatus(Action<string> printMessage)
    {
        printMessage($" {ChatColors.Yellow}Demo status:");

        // HLTV recording
        var tvEnabled = CstvManager.IsTvEnabled;
        var tvColor = tvEnabled ? ChatColors.Green : ChatColors.LightRed;
        printMessage($" {ChatColors.Grey}HLTV recording: {tvColor}{tvEnabled}");

        // TV delay
        var delay = CstvManager.GetTvDelay();
        printMessage($" {ChatColors.Grey}TV delay: {ChatColors.Gold}{delay}s");

        // Upload status
        if (!MatchConfig.DemoUploadEnabled)
        {
            printMessage($" {ChatColors.Grey}Demo upload: {ChatColors.LightRed}False");
        }
        else if (string.IsNullOrEmpty(MatchConfig.DemoUploadUrl))
        {
            printMessage($" {ChatColors.Grey}Demo upload: {ChatColors.LightRed}Error {ChatColors.Grey}(missing MATCHUP_DEMO_UPLOAD_URL)");
        }
        else if (string.IsNullOrEmpty(MatchConfig.DemoUploadToken))
        {
            printMessage($" {ChatColors.Grey}Demo upload: {ChatColors.LightRed}Error {ChatColors.Grey}(missing MATCHUP_DEMO_UPLOAD_TOKEN)");
        }
        else
        {
            printMessage($" {ChatColors.Grey}Demo upload: {ChatColors.Green}True");
        }
    }

    public static string? GetMatchStatusJson()
    {
        var gameState = StateMachine.GetCurrentGameState();

        try
        {
            // Get team scores from team entities
            var teamEntities = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
            var ctScore = 0;
            var tScore = 0;
            foreach (var team in teamEntities)
            {
                if (team.TeamNum == (byte)CsTeam.CounterTerrorist)
                    ctScore = team.Score;
                else if (team.TeamNum == (byte)CsTeam.Terrorist)
                    tScore = team.Score;
            }

            // Build player list
            var players = new List<object>();
            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
            foreach (var p in playerEntities)
            {
                if (!p.IsValid) continue;

                var side = p.TeamNum switch
                {
                    (byte)CsTeam.Terrorist => "T",
                    (byte)CsTeam.CounterTerrorist => "CT",
                    (byte)CsTeam.Spectator => "spectator",
                    _ => "other"
                };

                players.Add(new
                {
                    name = p.PlayerName,
                    steamId64 = p.SteamID,
                    isBot = p.IsBot,
                    side
                });
            }

            var status = new
            {
                state = gameState.ToString().ToLower(),
                map = new
                {
                    name = Server.MapName,
                    workshopId = MatchConfig.Map.WorkshopId
                },
                score = new
                {
                    ct = ctScore,
                    t = tScore
                },
                config = new
                {
                    playersPerTeam = MatchConfig.PlayersPerTeam,
                    maxPlayers = MatchConfig.PlayersPerTeam * 2,
                    knifeRound = MatchConfig.KnifeRound
                },
                players
            };

            return JsonSerializer.Serialize(status, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        catch (Exception e)
        {
            Console.WriteLine($"Error while getting match status: {e.Message}");
            return null;
        }
    }
}