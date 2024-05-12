using AviDrugZ.Models.VRC;
using AviDrugZ.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Windows;
using VRChat.API.Model;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using RestSharp.Extensions;

namespace AviDrugZ.Modules
{
    public class LiveScanner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LiveScanner));
        private FileSystemWatcher watcher;
        private string baseCacheLocation;

        public ObservableCollection<AvatarModel> Avatars { get; private set; }

        public LiveScanner(string cacheLocation)
        {
            baseCacheLocation = cacheLocation;
            Avatars = new ObservableCollection<AvatarModel>();
            SetupWatcher();
        }

        public static ObservableCollection<AvatarModel> LoadScansFromSession(string sessionFileName)
        {
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scans");
            string sessionFilePath = Path.Combine(directoryPath, sessionFileName);

            if (System.IO.File.Exists(sessionFilePath))
            {
                string content = System.IO.File.ReadAllText(sessionFilePath);
                var avatars = JsonConvert.DeserializeObject<ObservableCollection<AvatarSimpleLog>>(content);
                
                var avatarsToReturn = new ObservableCollection<AvatarModel>();

                //Conver to full model
                foreach (var avatar in avatars)
                {
                    avatarsToReturn.Add(new AvatarModel
                    {
                        AvatarID = avatar.Id,
                        AvatarName = avatar.AvatarName,
                        AuthorId = avatar.AuthorId,
                        AuthorName = avatar.AuthorName,
                        Description = avatar.Description,
                        ThumbnailUrl = avatar.ThumbnailUrl,
                        QuestSupported = avatar.SupportedPlatforms == 1,
                        DateAdded = avatar.DateAdded,
                        Version = avatar.Version.HasValue()?avatar.Version:"",
                        CacheLocation = avatar.CacheLocation,
                        DateDownloaded = avatar.DateDownloaded
                    });
                }

                return avatarsToReturn;
            }

            return new ObservableCollection<AvatarModel>();
        }

        private void SetupWatcher()
        {
            watcher = new FileSystemWatcher(baseCacheLocation);

            watcher.Created += OnNewDirectoryDetected;
            watcher.EnableRaisingEvents = true;
        }

        private async void OnNewDirectoryDetected(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Wait a second
                await Task.Delay(1000);

                string[] subdirectories = Directory.GetDirectories(e.FullPath);
                foreach (var subdirectory in subdirectories)
                {
                    string dataFilePath = Path.Combine(subdirectory, "__data");
                    if (System.IO.File.Exists(dataFilePath))
                    {
                        CacheResult result = KlauenUtils.ReadUntilFieldValue(dataFilePath, "prefab-id-v1");



                        if (result != null)
                        {
                            //Check if its a valid avatarID like this avtr_4086f5bc-3d9f-4367-ae38-74d7284e0403
                            if (Regex.IsMatch("avtr_" + result.AvatarID, @"^avtr_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"))
                            { 
                                var newAvatar = new AvatarModel { AvatarID = result.AvatarID, CacheLocation = dataFilePath };
                                Application.Current.Dispatcher.Invoke(() => Avatars.Add(newAvatar)); // Ensures thread safety
                                log.Info($"New avatar found: {result.AvatarID}");
                            }
                            else
                            {
                                //Invalid avatarID
                                log.Info($"Invalid avatarID found: {result.AvatarID}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error processing new directory: " + ex.Message);
            }
        }

        public static class AvatarLogger
        {
            private static string sessionFilePath;
            private static readonly object fileLock = new object();

            public static void InitializeSession()
            {
                string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scans");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                sessionFilePath = Path.Combine(directoryPath, $"Session_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                System.IO.File.WriteAllText(sessionFilePath, "[]"); // Initialize empty JSON array
            }

            public static void LogAvatarModel(AvatarModel model)
            {
                AvatarSimpleLog simpleModel = ConvertToSimpleLogModel(model);

                lock (fileLock) // Ensuring thread safety
                {
                    var existingContent = System.IO.File.ReadAllText(sessionFilePath);
                    List<AvatarSimpleLog> existingModels = JsonConvert.DeserializeObject<List<AvatarSimpleLog>>(existingContent);

                    if (existingModels == null)
                    {
                        existingModels = new List<AvatarSimpleLog>();
                    }

                    existingModels.Add(simpleModel);

                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    System.IO.File.WriteAllText(sessionFilePath, JsonConvert.SerializeObject(existingModels, settings));
                }
            }

            public static AvatarSimpleLog ConvertToSimpleLogModel(AvatarModel model)
            {
                return new AvatarSimpleLog
                {
                    Id = model.AvatarID,
                    AvatarName = model.AvatarName,
                    AuthorId = model.AuthorId,
                    AuthorName = model.AuthorName,
                    Description = model.Description,
                    ThumbnailUrl = model.ThumbnailUrl,
                    SupportedPlatforms = model.QuestSupported ? 1 : 0, // Convert boolean to int if necessary
                    DateAdded = model.DateAdded,
                    Version = model.Version,
                    UnityVersion = model.Version, // Assuming UnityVersion is stored in Version
                    CacheLocation = model.CacheLocation,
                    DateDownloaded = model.DateDownloaded
                };
            }

            public static AvatarSimple ConvertToSimpleModel(AvatarModel model)
            {
                return new AvatarSimple
                {
                    Id = model.AvatarID,
                    AvatarName = model.AvatarName,
                    AuthorId = model.AuthorId,
                    AuthorName = model.AuthorName,
                    Description = model.Description,
                    ThumbnailUrl = model.ThumbnailUrl,
                    SupportedPlatforms = model.QuestSupported ? 1 : 0, // Convert boolean to int if necessary
                    DateAdded = model.DateAdded,
                    Version = model.Version,
                    UnityVersion = model.Version // Assuming UnityVersion is stored in Version
                };
            }

            public static void LogException(Exception ex)
            {
                System.IO.File.AppendAllText(sessionFilePath, JsonConvert.SerializeObject(new { Error = ex.Message }, Formatting.Indented));
            }
        }



        public void StopWatching()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
    }
}
