using CounterStrikeSharp.API;

namespace MatchUp;

public static class Utils
{
    public static async void DelayedCall(TimeSpan delay, Action callback)
    {
        await Task.Delay(delay);
        Server.NextWorldUpdate(() => { callback(); });
    }
}