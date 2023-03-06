using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Policy;
using VRChat.API.Model;
using log4net;
using aviDrug;
using AviDrugZ.Models;
using System.Diagnostics;
using System.Windows.Markup;
using AviDrugZ.Views;

namespace AviDrugZ.Modules
{
    //Very unperformant cache scanner&duplicater
    public class CacheScanner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CacheScanner));
        public CacheScanner()
        {

        }

        static int avatarCount = 0;
        static int worldCount = 0;

        public async Task<List<string>> scanCacheFast(int limit, string cacheLocation, NyanLoading? loadingView = null)
        {
            int aviFound = 0;
            List<AvatarModel> avis = new List<AvatarModel>();

            Stopwatch stop = new Stopwatch();
            stop.Start();

            List<Task<string>> tasks = new List<Task<string>>();
            int i = 0;
            Dictionary<string, DateTime> locations = getCacheLocations(cacheLocation);
            NyanLoading.instance.setMaxProgress(locations.Count);

            foreach (string cache in locations.Keys)
            {
                i++;
                if (i == limit) { break; }
                tasks.Add(Task.Run(() => KlauenUtils.ReadUntilFieldValue(cache, "prefab-id-v1", loadingView)));
            }


            Task.WaitAll(tasks.ToArray());

            List<string> avatarIds = new List<string>();

            string outputBuffer = "";


            foreach (Task<string> task in tasks)
            {
                string data = task.Result;
                if (data != null)
                {
                    //ID has 36 characters
                    if (data.Length == 36)
                    {
                        data = "avtr_" + data;
                        avatarIds.Add(data);
                        outputBuffer += data + ";";
                        log.Info("Found avi " + data);
                        aviFound++;
                    }
                    else
                    {
                        log.Info("Found malformed ID");
                    }
                }
            }


            System.IO.File.WriteAllText("avatarLog.txt", outputBuffer);
            stop.Stop();
            log.Info("Found " + aviFound + " avatars in " + stop.ElapsedMilliseconds + "ms");
            return avatarIds;
        }




        public static Dictionary<string, DateTime> getCacheLocations(string path)
        {

            Dictionary<string, DateTime> dataLocations = new Dictionary<string, DateTime>();


            string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // string[] dirs = Directory.GetDirectories($"{cacheFolder}\\..\\LocalLow\\VRChat\\VRChat\\Cache-WindowsPlayer");
            string[] dirs = Directory.GetDirectories(path);
            foreach (string item in dirs)
            {
                try
                {
                    string[] dataFolders = Directory.GetDirectories(item);
                    if (dataFolders.Length > 0)
                    {
                        //Has data cache ?
                        string cacheDataPath = dataFolders[0] + "\\__data";
                        if (System.IO.File.Exists(cacheDataPath))
                        {
                            FileInfo info = new FileInfo(cacheDataPath);
                            dataLocations.Add(cacheDataPath, info.CreationTime);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    continue;
                }

            }

            var dicList = dataLocations.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return dicList;
        }


    }
}
