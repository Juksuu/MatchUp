using System.Net;
using System.Text.Json;
using CounterStrikeSharp.API;

namespace MatchUp;

public static class WebhookClient
{
    private static readonly HttpClient _client = new HttpClient();

   public static void PostStatus(string status)
{
    Task.Run(async () =>
    {
        var webhookUrl = PelipajaConfig.WebhookUrl;
        var matchId = PelipajaConfig.MatchId;

        if (string.IsNullOrEmpty(webhookUrl) || string.IsNullOrEmpty(matchId))
        {
            Console.WriteLine("[Pelipaja] WebhookUrl or MatchId not set, skipping webhook");
            return;
        }

        try
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {PelipajaConfig.ApiSecret}");
            var json = JsonSerializer.Serialize(new { status });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _client.PostAsync($"{webhookUrl}/api/matches/{matchId}/status", content);
            Console.WriteLine($"[Pelipaja] Posted status: {status}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Pelipaja] Failed to post status: {e.Message}");
        }
    });
    }
}