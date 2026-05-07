using System.Net;
using System.Text.Json;
using CounterStrikeSharp.API;

namespace MatchUp;

public static class WebhookClient
{
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(5),
        MaxResponseContentBufferSize = 1024
    };

    private static readonly Queue<string> _statusQueue = new();
    private static bool _isProcessing = false;
    private static readonly object _queueLock = new();

    public static void PostStatus(string status)
    {
        lock (_queueLock)
        {
            _statusQueue.Enqueue(status);
        }
        _ = ProcessQueueAsync();
    }

    private static async Task ProcessQueueAsync()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        try
        {
            while (true)
            {
                string status;
                lock (_queueLock)
                {
                    if (_statusQueue.Count == 0) break;
                    status = _statusQueue.Dequeue();
                }

                await SendStatusAsync(status);
                await Task.Delay(100); // Small delay between requests
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private static async Task SendStatusAsync(string status)
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

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _client.PostAsync($"{webhookUrl}/api/matches/{matchId}/status", content, cts.Token);
            Console.WriteLine($"[Pelipaja] Posted status: {status}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Pelipaja] Webhook request timed out for status: {status}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Pelipaja] Failed to post status '{status}': {e.Message}");
        }
    }
}