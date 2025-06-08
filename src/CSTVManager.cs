using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace MatchUp;

public static class CSTVManager
{
    static bool isTvEnabled = getTvEnabled();

    public static int getTvDelay()
    {
        if (isTvEnabled)
        {
            // TODO: Test which delay should be used here, tv_delay or tv_delay1
            var tvDelayCvar = ConVar.Find("tv_delay");
            if (tvDelayCvar != null)
            {
                return tvDelayCvar.GetPrimitiveValue<int>();
            }
        }

        return 0;
    }

    public static void startDemoRecording()
    {
        if (isTvEnabled)
        {
            var delay = getTvDelay();
            Utils.DelayedCall(TimeSpan.FromSeconds(delay), () =>
            {
                string formattedTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
                string demoName = $"{formattedTime}-{Server.MapName}";
                Server.ExecuteCommand($"tv_record {demoName}");
            });
        }
    }

    public static void stopDemoRecording()
    {
        if (isTvEnabled)
        {
            Server.ExecuteCommand("tv_stoprecord");
        }
    }

    private static bool getTvEnabled()
    {
        var tvEnableCVar = ConVar.Find("tv_enable");
        return tvEnableCVar != null && tvEnableCVar.GetPrimitiveValue<bool>();
    }
}