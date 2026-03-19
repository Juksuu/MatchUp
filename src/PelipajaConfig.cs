namespace MatchUp;

public class TeamInfo {
    public required string Name { get; set; }
    public List<string> Players { get; set; } = [];
    
}


public static class PelipajaConfig {
    // these are set once the container starts then dont change
    public static string? WebhookUrl { get; private set; }
    public static string? ApiSecret { get; private set; }
    
    // these are set when HTTP config arrives from Next.js
    public static string Mode { get; private set; } = "manual"; // this is a switch between matchup old config and new pelipaja.net config
    public static string? MatchId { get; private set; }
    public static TeamInfo? Team1 { get; private set; }
    public static TeamInfo? Team2 { get; private set; } 

    public static void Load()
    {
        WebhookUrl = Environment.GetEnvironmentVariable("MATCHUP_WEBHOOK_URL");
        ApiSecret = Environment.GetEnvironmentVariable("MATCHUP_API_SECRET");

        Console.WriteLine($"[Pelipaja] WebhookUrl: {WebhookUrl}");
        Console.WriteLine($"[Pelipaja] ApiSecret set: {ApiSecret!=null}"); // sends a boolean true false
    }

    public static void SetMatchConfig(string mode, string matchId, TeamInfo team1, TeamInfo team2)
    {
        Mode = mode;
        MatchId = matchId;
        Team1 = team1;
        Team2 =  team2;

        Console.WriteLine($"[Pelipaja] Config received - Mode: {Mode}, MatchId: {MatchId}");
        Console.WriteLine($"[Pelipaja] Team1: {Team1.Name}, Team2: {Team2.Name}");
    }
    
}