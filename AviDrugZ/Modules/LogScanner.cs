using AviDrugZ.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AviDrugZ.Modules
{
    public class LogScanner
    {
        private string logLocation;
        private FileSystemWatcher fileWatcher;
        private CancellationTokenSource cancellationTokenSource;

        // Exposed collection that external code can subscribe to.
        public ObservableCollection<AvatarModel> Avatars { get; private set; }

        public LogScanner(string logLocation = null)
        {
            // If no location is specified, build the path from the current user profile.
            if (string.IsNullOrEmpty(logLocation))
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                // Build: C:\Users\<username>\AppData\LocalLow\VRChat\VRChat
                this.logLocation = Path.Combine(userProfile, "AppData", "LocalLow", "VRChat", "VRChat");
            }
            else
            {
                this.logLocation = logLocation;
            }

            Avatars = new ObservableCollection<AvatarModel>();
        }

        /// <summary>
        /// Starts the log scanning. This sets up the file watcher and tails the current log file.
        /// </summary>
        public void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();

            // Setup FileSystemWatcher for *.txt files in the log directory.
            fileWatcher = new FileSystemWatcher(logLocation, "*.txt");
            fileWatcher.Created += OnLogFileCreated;
            fileWatcher.Changed += OnLogFileChanged; // Also listen to changes.
            fileWatcher.EnableRaisingEvents = true;

            // Tail the most recent log file (if one exists) at startup.
            string latestLogFile = GetLatestLogFile();
            if (!string.IsNullOrEmpty(latestLogFile))
            {
                Console.WriteLine($"Starting tail on existing log: {latestLogFile}");
                StartTailTask(latestLogFile, cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops the scanning process.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource?.Cancel();
            fileWatcher?.Dispose();
        }

        /// <summary>
        /// Returns the most recent *.txt file in the log directory.
        /// </summary>
        private string GetLatestLogFile()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(logLocation);
                var files = di.GetFiles("*.txt");
                if (files.Length == 0)
                    return null;
                var latestFile = files.OrderByDescending(f => f.LastWriteTime).First();
                return latestFile.FullName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving latest log file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Event handler when a new log file is created.
        /// </summary>
        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"[Watcher] New log file detected: {e.FullPath}");
            StartTailTask(e.FullPath, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Event handler when an existing log file is changed.
        /// This is useful if VRChat updates the same log file.
        /// </summary>
        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"[Watcher] Log file changed: {e.FullPath}");
            // If the file is updated, the current tail loop should pick up the new lines.
            // If not, our periodic re-opening (below) will re-open the file.
        }

        /// <summary>
        /// Starts a background task that tails a file.
        /// </summary>
        private async void StartTailTask(string filePath, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[TailTask] Starting tail for file: {filePath}");
            await Task.Run(() => TailFile(filePath, cancellationToken));
        }

        /// <summary>
        /// Continuously reads new lines from the file and processes them.
        /// If no new lines are detected for a set period, the file is re-opened.
        /// </summary>
        private async Task TailFile(string filePath, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs))
                    {
                        Console.WriteLine($"[TailFile] Reading full file first: {filePath}");

                        //  1. Read all existing lines first
                        //while (!reader.EndOfStream)
                        //{
                        //    string line = await reader.ReadLineAsync();
                        //    if (line != null)
                        //        ProcessLine(line);
                        //}

                        //put reader to end of file
                        fs.Seek(0, SeekOrigin.End);

                        Console.WriteLine($"[TailFile] Now tailing new lines in: {filePath}");

                        // 2. Then continue tailing new lines
                        int noNewLinesCount = 0;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            string line = await reader.ReadLineAsync();
                            if (line != null)
                            {
                                noNewLinesCount = 0;
                                ProcessLine(line);
                            }
                            else
                            {
                                noNewLinesCount++;
                                if (noNewLinesCount >= 25)
                                {
                                    Console.WriteLine("[TailFile] No new lines detected for 5 seconds, re-opening file.");
                                    break;
                                }
                                await Task.Delay(200, cancellationToken);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[TailFile] Tailing cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in tailing loop: {ex.Message}");
                }

                await Task.Delay(500, cancellationToken);
            }
        }


        /// <summary>
        /// Checks a log line for API calls and extracts the avatarID if present.
        /// It handles "select", "get", and "fetched from cache" events.
        /// </summary>
        private void ProcessLine(string line)
        {
            // Pattern 4: "fetched from apimodel" events.
            //Regex regexApi = new Regex(@"Fetched ApiModel with id (avtr_[a-f0-9\-]+) from cache", RegexOptions.IgnoreCase);
            //Match matchApi = regexApi.Match(line);
            //if (matchApi.Success)
            //{
            //    string fullId = matchApi.Groups[1].Value;
            //    string avatarId = fullId.Replace("avtr_", "", StringComparison.OrdinalIgnoreCase);
            //    Console.WriteLine($"[ProcessLine] Avatar detected (cache): {avatarId}");
            //    if (!Avatars.Any(a => a.AvatarID == avatarId))
            //    {
            //        Avatars.Add(new AvatarModel { AvatarID = avatarId, CacheLocation = "" });
            //    }
            //    return;
            //}

            // Pattern 1: "select" events.
            Regex regexSelect = new Regex(@"avatars/(avtr_[a-f0-9\-]+)/select", RegexOptions.IgnoreCase);
            Match matchSelect = regexSelect.Match(line);
            if (matchSelect.Success)
            {
                string fullId = matchSelect.Groups[1].Value;
                string avatarId = fullId.Replace("avtr_", "", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"[ProcessLine] Avatar detected (select): {avatarId}");
                if (!Avatars.Any(a => a.AvatarID == avatarId))
                {
                    Avatars.Add(new AvatarModel { AvatarID = avatarId, CacheLocation = "" });
                }
                return;
            }

            // Pattern 2: "get" events.
            Regex regexGet = new Regex(@"Sending Get request to https?:\/\/api\.vrchat\.cloud\/api\/1\/avatars/(avtr_[a-f0-9\-]+)", RegexOptions.IgnoreCase);
            Match matchGet = regexGet.Match(line);
            if (matchGet.Success)
            {
                string fullId = matchGet.Groups[1].Value;
                string avatarId = fullId.Replace("avtr_", "", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"[ProcessLine] Avatar detected (get): {avatarId}");
                if (!Avatars.Any(a => a.AvatarID == avatarId))
                {
                    Avatars.Add(new AvatarModel { AvatarID = avatarId, CacheLocation = "" });
                }
                return;
            }

            // Pattern 3: "fetched from cache" events.
            //Regex regexCache = new Regex(@"Fetched ApiAvatar with id (avtr_[a-f0-9\-]+) from cache", RegexOptions.IgnoreCase);
            //Match matchCache = regexCache.Match(line);
            //if (matchCache.Success)
            //{
            //    string fullId = matchCache.Groups[1].Value;
            //    string avatarId = fullId.Replace("avtr_", "", StringComparison.OrdinalIgnoreCase);
            //    Console.WriteLine($"[ProcessLine] Avatar detected (cache): {avatarId}");
            //    if (!Avatars.Any(a => a.AvatarID == avatarId))
            //    {
            //        Avatars.Add(new AvatarModel { AvatarID = avatarId, CacheLocation = "" });
            //    }
            //    return;
            //}




        }
    }
}
