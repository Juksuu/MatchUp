using System.Net;
using System.Text.Json;

namespace MatchUp;


public static class HttpServer
{
    private static HttpListener? _listener;

    public static void Start()
    {
        try
        {
            var port = Environment.GetEnvironmentVariable("MATCHUP_API_PORT") ?? "27090";

            _listener =  new HttpListener();
            _listener.Prefixes.Add($"http://*:{port}/");
            _listener.Start();
        }
        catch(Exception e)
        {
            Console.WriteLine($"Error during HTTPServer Start(): {e.Message}");
        }
    }
}