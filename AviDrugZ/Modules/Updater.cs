using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AviDrugZ.Modules
{
    internal class Updater
    {
        public string latestRelease = "https://api.github.com/repos/Klauensprung/AviDrugZ/releases/latest";
        public string latestDownload;
        public bool checkForUpdate()
        {
            //Get API GET Response from latestRelease
            HttpClient client = new HttpClient();

            //Set Browser agent to AviDrugZ Updater
            client.DefaultRequestHeaders.Add("User-Agent", "AviDrugZ Updater");

            string response = client.GetStringAsync(latestRelease).GetAwaiter().GetResult();

            //Convert response to json object
            var json = Newtonsoft.Json.Linq.JObject.Parse(response);

            //Get latest version from json object
            Version latestVersion = new Version(json["name"].ToString());
            latestDownload = json["assets"][0]["browser_download_url"].ToString();
            
            Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                           
            if (currentVersion.CompareTo(latestVersion)<0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
