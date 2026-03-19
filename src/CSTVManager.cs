using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace MatchUp;

public static class CstvManager
{
    public static readonly bool IsTvEnabled = GetTvEnabled();
    private static readonly HttpClient HttpClient = new();

    // Holds the file name of the active demo (without extension)
    private static string? _activeDemoName;

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
            _activeDemoName = demoName;
            Server.ExecuteCommand($"tv_record {demoName}");
        });
    }

    public static void StopDemoRecording(string scores)
    {
        if (!IsTvEnabled)
        {
            return;
        }

        Server.ExecuteCommand("tv_stoprecord");

        var demoName = _activeDemoName;
        _activeDemoName = null;

        if (!MatchConfig.DemoUploadEnabled || string.IsNullOrEmpty(MatchConfig.DemoUploadUrl) || string.IsNullOrEmpty(demoName))
        {
            return;
        }

        // Wait a bit for the demo file to be flushed to disk before uploading
        var demoFileName = $"{demoName}.dem";
        var uploadFileName = $"{demoName}-{scores}.dem";
        var demoFilePath = Path.Join(Server.GameDirectory, "csgo", demoFileName);

        Utils.DelayedCall(TimeSpan.FromSeconds(15), () =>
        {
            Task.Run(async () =>
            {
                await UploadDemoAsync(demoFilePath, uploadFileName);
            });
        });
    }

    private static async Task UploadDemoAsync(string filePath, string fileName)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[MatchUp] Demo file not found for upload: {filePath}");
                return;
            }

            // Public blob URL
            var blobUrl = $"{MatchConfig.DemoUploadUrl}/{fileName}";

            // Full URL with SAS token
            var uploadUrl = string.IsNullOrEmpty(MatchConfig.DemoUploadToken)
                ? blobUrl
                : $"{blobUrl}?{MatchConfig.DemoUploadToken}";

            Console.WriteLine($"[MatchUp] Uploading demo to Azure: {fileName}");

            await using var fileStream = File.OpenRead(filePath);
            using var content = new StreamContent(fileStream);
            content.Headers.Add("x-ms-blob-type", "BlockBlob");

            using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            request.Content = content;

            using var response = await HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[MatchUp] Demo uploaded successfully: {fileName}");
                Console.WriteLine($"[MatchUp] Demo URL: {blobUrl}");
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"[MatchUp] Demo upload failed ({response.StatusCode}): {body}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MatchUp] Demo upload error: {ex.Message}");
        }
    }

    private static bool GetTvEnabled()
    {
        var tvEnableCVar = ConVar.Find("tv_enable");
        return tvEnableCVar != null && tvEnableCVar.GetPrimitiveValue<bool>();
    }
}