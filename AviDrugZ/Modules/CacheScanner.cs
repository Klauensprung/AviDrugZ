using AssetsTools;
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
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Security.Policy;
using VRChat.API.Model;
using log4net;
using aviDrug;
using AviDrugZ.Models;
using System.Diagnostics;
using System.Windows.Markup;

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

        public async Task<List<string>> scanCacheFast(int limit, string cacheLocation)
        {
            int aviFound = 0;
            List<AvatarModel> avis = new List<AvatarModel>();

            Stopwatch stop = new Stopwatch();
            stop.Start();

            List<Task<string>> tasks = new List<Task<string>>();
            int i = 0;
            foreach (string cache in getCacheLocations(cacheLocation).Keys)
            {
                i++;
                if (i == limit) { break; }
                tasks.Add(Task.Run(() =>   KlauenUtils.ReadUntilFieldValue(cache, "prefab-id-v1")));
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


            System.IO.File.WriteAllText("avatarLog.txt",outputBuffer);
            stop.Stop();
            log.Info("Found " + aviFound + " avatars in " + stop.ElapsedMilliseconds + "ms");
            return avatarIds;
        }

        public void runScanMulti(string tempLocation)
        {
            List<string> cacheLocations = getCacheLocations("H:\\vrccache\\Cache-WindowsPlayer").Keys.ToList();

            avatarCount = 0;
            worldCount = 0;

            List<Task<ScanResult>> tasks = new List<Task<ScanResult>>();

            foreach (string item in cacheLocations)
            {
                tasks.Add(Task.Run(() => ScanCacheRoutine(item,tempLocation)));
            }

            Task.WaitAll(tasks.ToArray());

            foreach (Task<ScanResult> task in tasks)
            {
                ScanResult scan = task.Result;
                if(scan.success)
                {

                    //Cleanup
                    if(System.IO.File.Exists(scan.path2))
                    {
                        System.IO.File.Delete(scan.path2);
                    }

                    foreach (VRCCacheScannerLogObject item in scan.result)
                    {
                        Console.WriteLine(item.ID);
                    }
                }
            }

            List<VRCCacheScannerLogObject> results = new List<VRCCacheScannerLogObject>();
            foreach (VRCCacheScannerLogObject result in results)
            {

            }

        }

        public class ScanResult
        {
            public string path2;
            public bool success = false;
            public List<VRCCacheScannerLogObject> result;

            public ScanResult(string path2, bool success, List<VRCCacheScannerLogObject> result)
            {
                this.path2 = path2;
                this.success = success;
                this.result = result;
            }
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
        public Task<ScanResult> ScanCacheRoutine(string path, string tempLocation)
        {
            string text = CalculateMD5(path);
            //if (!scanAll && fileHashes.Contains(text))
            //{
            //    return null;
            //}

            string programOpenTime = DateTime.Today.ToString();
            List<VRCCacheScannerLogObject> result = new List<VRCCacheScannerLogObject>();

            DateTime creationTime = System.IO.File.GetCreationTime(path);
            string path2 = "";
            using(FileStream stream = System.IO.File.OpenRead(path))
            {
                AssetBundleFile assetBundleFile = new AssetBundleFile();
                try
                {
                    assetBundleFile.Read(new AssetsFileReader(stream));
                } catch
                {
                    assetBundleFile.DataReader.Dispose();
                    Console.Write(
                        (creationTime, "######################################################\nFile at '" +
                            path +
                            "' unreadable.\n"));
                    return Task.FromResult(new ScanResult(path2, false, result));
                }
                if(assetBundleFile.Header == null)
                {
                    Console.Write(
                        (creationTime, "######################################################\nAssetBundle '" +
                            path +
                            "' unreadable.\n"));
                    return Task.FromResult(new ScanResult(path2, false, result));
                }

                path2 = Path.Combine(tempLocation, Guid.NewGuid().ToString());

                try
                {
                    using FileStream fileStream = System.IO.File.Open(
                        path2,
                        FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite);
                    assetBundleFile.Unpack(new AssetsFileWriter(fileStream));
                } catch
                {
                    Console.Write(
                        (creationTime, "######################################################\nAssetBundle '" +
                            path +
                            "' failed to unpack.\n"));
                    return Task.FromResult(new ScanResult(path2, false, result));
                }
            }
            AssetsManager assetsManager = new AssetsManager();
            BundleFileInstance bundleFileInstance = assetsManager.LoadBundleFile(path2);
            int num = bundleFileInstance.file.BlockAndDirInfo.DirectoryInfos.Length;
            bundleFileInstance.BundleStream.Position = 0L;
            List<AssetsFileInstance> list = new List<AssetsFileInstance>();
            for(int i = 0; i < num; i++)
            {
                try
                {
                    AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFileFromBundle(
                        bundleFileInstance,
                        i,
                        loadDeps: true);
                    if(assetsFileInstance != null)
                    {
                        list.Add(assetsFileInstance);
                    }
                } catch
                {
                }
            }
            if(list.Count == 0)
            {
                assetsManager.UnloadAll();
                Console.Write(
                    (creationTime, "######################################################\nAssetBundle '" +
                        path +
                        "' doesn't have asset files.\n"));
                return Task.FromResult(new ScanResult(path2, false, result)); 
            }
            bool flag = false;
            string text2 = " ";
            foreach(AssetsFileInstance item in list)
            {
                foreach(AssetFileInfo item2 in item.file.GetAssetsOfType(AssetClassID.AssetBundle))
                {
                    try
                    {
                        AssetTypeValueField baseField = assetsManager.GetBaseField(item, item2);
                        text2 = baseField["m_Name"].AsString;
                        if(text2.EndsWith(".vrcw"))
                        {
                            string text3 = text2;
                            text2 = text3.Substring(0, text3.Length - 5).Replace("scene-standalonewindows64-", "scene-");
                        } else if(text2.EndsWith(".unity3d"))
                        {
                            string text3 = text2;
                            text2 = text3.Substring(0, text3.Length - 8);
                        }
                        if(baseField["m_IsStreamedSceneAssetBundle"].AsBool)
                        {
                            flag = true;
                            break;
                        }
                    } catch
                    {
                        assetsManager.UnloadAll();
                        Console.Write(
                            (creationTime, "######################################################\nCan't read AssetBundle at '" +
                                path +
                                "'\n"));
                        return Task.FromResult(new ScanResult(path2, false, result));
                    }
                }
            }
            if(text2 == " ")
            {
                text2 = "";
            }
            string content = "avatar";
            if(flag)
            {
                content = "world";
            }
            long? num2 = null;
            foreach(AssetsFileInstance item3 in list)
            {
                foreach(AssetFileInfo item4 in item3.file.GetAssetsOfType(AssetClassID.MonoScript))
                {
                    AssetTypeValueField assetTypeValueField = null;
                    try
                    {
                        assetTypeValueField = assetsManager.GetBaseField(item3, item4);
                    } catch
                    {
                        assetsManager.UnloadAll();
                        Console.Write(
                            (creationTime, "######################################################\nBroken (crasher?) AssetBundle '" +
                                path +
                                "'. Can't read MonoScripts\n"));
                        result.Add(
                            new VRCCacheScannerLogObject(
                                programOpenTime,
                                creationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                path,
                                text,
                                content,
                                text2,
                                ""));
                        return Task.FromResult(new ScanResult(path2, false, result));
                    }
                    if(assetTypeValueField["m_Name"].AsString.EndsWith("PipelineManager"))
                    {
                        num2 = item4.PathId;
                        break;
                    }
                }
                if(num2.HasValue)
                {
                    break;
                }
            }
            if(!num2.HasValue)
            {
                assetsManager.UnloadAll();
                Console.Write(
                    (creationTime, "######################################################\nDidn't find Pipeline Manager script in AssetBundle '" +
                        path +
                        "'.\n"));
                result.Add(
                    new VRCCacheScannerLogObject(
                        programOpenTime,
                        creationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        path,
                        text,
                        content,
                        text2,
                        ""));
                return Task.FromResult(new ScanResult(path2, false, result));
            }
            foreach(AssetsFileInstance item5 in list)
            {
                foreach(AssetFileInfo item6 in item5.file.GetAssetsOfType(AssetClassID.MonoBehaviour))
                {
                    AssetTypeValueField assetTypeValueField2 = null;
                    try
                    {
                        assetTypeValueField2 = assetsManager.GetBaseField(item5, item6);
                    } catch
                    {
                        continue;
                    }
                    if(assetTypeValueField2["m_Script"]["m_PathID"].AsLong != num2)
                    {
                        continue;
                    }
                    AssetTypeValue value = assetTypeValueField2["blueprintId"].Value;
                    if(value != null && !string.IsNullOrWhiteSpace(value.AsString))
                    {
                        string asString = value.AsString;
                        if(flag)
                        {
                            Interlocked.Increment(ref worldCount);
                        } else
                        {
                            Interlocked.Increment(ref avatarCount);
                        }
                        result.Add(
                            new VRCCacheScannerLogObject(
                                programOpenTime,
                                creationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                path,
                                text,
                                content,
                                text2,
                                asString));
                        assetsManager.UnloadAll();
                        return Task.FromResult(new ScanResult(path2, true, result));
                    }
                }
            }
            assetsManager.UnloadAll();
            Console.Write(
                (creationTime, "######################################################\nDidn't find Pipeline Manager MonoBehaviour in AssetBundle '" +
                    path +
                    "'.\n"));
            result.Add(
                new VRCCacheScannerLogObject(
                    programOpenTime,
                    creationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    path,
                    text,
                    content,
                    text2,
                    ""));
            return Task.FromResult(new ScanResult(path2, true, result));
        }

        private static string CalculateMD5(string path)
        {
            //Calculate MD5 hash from file at the given path

            using (var md5 = MD5.Create())
            using (var stream = System.IO.File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
