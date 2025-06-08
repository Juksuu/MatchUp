using CounterStrikeSharp.API;

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
}