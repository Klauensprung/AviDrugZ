using AviDrugZ.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AviDrugZ.Modules
{
    public class AmplitudeCacheScanner
    {
        private string cacheFilePath;
        private DateTime lastModifiedTime;
        private CancellationTokenSource cancellationTokenSource;

        public ObservableCollection<AvatarModel> Avatars { get; private set; }

        public AmplitudeCacheScanner(string customPath = null)
        {
            string tempPath = Environment.GetEnvironmentVariable("TEMP");
            cacheFilePath = customPath ?? Path.Combine(tempPath, "VRChat", "VRChat", "amplitude.cache");

            Avatars = new ObservableCollection<AvatarModel>();
        }

        public void Start()
        {
            if (!File.Exists(cacheFilePath))
            {
                Console.WriteLine($"[AmplitudeScanner] File does not exist: {cacheFilePath}");
                return;
            }

            lastModifiedTime = File.GetLastWriteTime(cacheFilePath);
            cancellationTokenSource = new CancellationTokenSource();

            // Full scan at startup
            Task.Run(() => ScanAsync());

            // Watch for changes
            Task.Run(() => MonitorLoop(cancellationTokenSource.Token));
        }

        public void Stop()
        {
            cancellationTokenSource?.Cancel();
        }

        private async Task MonitorLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (File.Exists(cacheFilePath))
                    {
                        DateTime currentModified = File.GetLastWriteTime(cacheFilePath);
                        if (currentModified > lastModifiedTime)
                        {
                            Console.WriteLine("[AmplitudeScanner] Detected file change, scanning...");
                            lastModifiedTime = currentModified;
                            await ScanAsync();
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AmplitudeScanner] Error in monitor loop: {ex.Message}");
                }
            }
        }

        private async Task ScanAsync()
        {
            try
            {
                string json = await File.ReadAllTextAsync(cacheFilePath);
                JsonArray root = JsonNode.Parse(json)?.AsArray();

                if (root == null)
                {
                    Console.WriteLine("[AmplitudeScanner] JSON is not an array.");
                    return;
                }

                foreach (var entry in root)
                {
                    var props = entry?["event_properties"];

                    // avatarIdsEncountered
                    var encountered = props?["avatarIdsEncountered"]?.AsArray();
                    if (encountered != null)
                    {
                        foreach (var idNode in encountered)
                            TryAddAvatar(idNode?.ToString());
                    }

                    // lastWorldAvatarChanges_NewAvatarIds
                    var changed = props?["lastWorldAvatarChanges_NewAvatarIds"]?.AsArray();
                    if (changed != null)
                    {
                        foreach (var idNode in changed)
                            TryAddAvatar(idNode?.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AmplitudeScanner] Error parsing cache: {ex.Message}");
            }
        }

        private void TryAddAvatar(string fullId)
        {
            if (string.IsNullOrEmpty(fullId) || !fullId.StartsWith("avtr_"))
                return;

            string shortId = fullId.Substring(5);

            if (!Avatars.Any(a => a.AvatarID == shortId))
            {
                Avatars.Add(new AvatarModel
                {
                    AvatarID = shortId,
                    CacheLocation = cacheFilePath
                });

                Console.WriteLine($"[AmplitudeScanner] New avatar found: {shortId}");
            }
        }
    }
}
