using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace MatchUp;

public static class CstvManager
{
    private static readonly bool IsTvEnabled = GetTvEnabled();

    public static int GetTvDelay()
    {
        if (!IsTvEnabled)
        {
            return 0;
        }

        // TODO: Test which delay should be used here, tv_delay or tv_delay1
        var tvDelayCvar = ConVar.Find("tv_delay");
        return tvDelayCvar?.GetPrimitiveValue<int>() ?? 0;
    }

    public static void StartDemoRecording()
    {
        if (!IsTvEnabled)
        {
            return;
        }

        var delay = GetTvDelay();
        Utils.DelayedCall(TimeSpan.FromSeconds(delay), () =>
        {
            var formattedTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            var demoName = $"{formattedTime}-{Server.MapName}";
            Server.ExecuteCommand($"tv_record {demoName}");
        });
    }

    public static void StopDemoRecording()
    {
        if (IsTvEnabled)
        {
            Server.ExecuteCommand("tv_stoprecord");
        }
    }

    private static bool GetTvEnabled()
    {
        var tvEnableCVar = ConVar.Find("tv_enable");
        return tvEnableCVar != null && tvEnableCVar.GetPrimitiveValue<bool>();
    }
}