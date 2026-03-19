using System.Net;
using System.Text.Json;
using CounterStrikeSharp.API;

namespace MatchUp;

public static class WebhookClient
{
    
    public static async Task PostStatus(string status)
    {
        var WebhookUrl = PelipajaConfig.WebhookUrl;
        var MatchId =  PelipajaConfig.MatchId;

        if (string.IsNullOrEmpty(WebhookUrl) || string.IsNullOrEmpty(MatchId))
        {
            Console.WriteLine("[Pelipaja] WebhookUrl or MatchId not set, skipping webhook.");
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer  {PelipajaConfig.ApiSecret}");
            var json = JsonSerializer.Serialize(new { status });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync($"{WebhookUrl}/api/matches/{MatchId}/status", content);
            Console.WriteLine($"[Pelipaja] Posted status: {status}");
        }
        catch(Exception e)
        {
            Console.WriteLine($"[Pelipaja] Failed to post status: {e.Message}");
        }
    }
}