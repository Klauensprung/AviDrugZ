using System.Diagnostics;
using System.IO.Compression;
using System.Net;

public class UpdateInstall
{

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting update process for AviDrugZ");
        
        
        string startLocation = args[0];
        // Create a new instance of the Stopwatch class, 
        // and start the timing. 
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        update(startLocation);
        // Stop timing. 
        stopWatch.Stop();

        // Write result. 
        Console.WriteLine("Time elapsed: {0}", stopWatch.Elapsed);
    }

    public static string latestRelease = "https://api.github.com/repos/Klauensprung/AviDrugZ/releases/latest";
    public static void update(string location)
    {
        //Get API GET Response from latestRelease
        HttpClient client = new HttpClient();

        //Set Browser agent to AviDrugZ Updater
        client.DefaultRequestHeaders.Add("User-Agent", "AviDrugZ Updater");

        string response = client.GetStringAsync(latestRelease).GetAwaiter().GetResult();

        var jsonResponse = System.Text.Json.JsonDocument.Parse(response);

        //Get changelog 
        string changelog = jsonResponse.RootElement
                            .GetProperty("body").ToString();

        //Get latest version from json object
        string latestVersion = jsonResponse.RootElement
                            .GetProperty("tag_name").ToString();

        //Get download url from json object
        string latestDownload = jsonResponse.RootElement
                            .GetProperty("assets").EnumerateArray().First().GetProperty("browser_download_url").ToString();

        //Get filename from json object
        string filename = jsonResponse.RootElement
                            .GetProperty("assets").EnumerateArray().First().GetProperty("name").ToString();

        //Print all relevant date from the response to the console
        Console.WriteLine("------------------------------");
        Console.WriteLine("Latest Version: " + latestVersion);
        Console.WriteLine("Changelog: " + changelog);
        Console.WriteLine("Download URL: " + latestDownload);
        Console.WriteLine("Filename: " + filename);        
        Console.WriteLine("------------------------------");
        

        


        string downloadFile = downloadUpdate(latestDownload);
        installUpdate(downloadFile, location);


    }


    public static void installUpdate(string path, string to)
    {
        Console.WriteLine("Installing update from " + path + " to " + to);
        string downloadPath = path;
        string extractPath = Path.Combine(to);

        //Extract to temp path
        ZipFile.ExtractToDirectory(downloadPath, extractPath);

        //Copy files in first folder in extractPath
        string[] dirs = Directory.GetDirectories(extractPath);
        string[] files = Directory.GetFiles(dirs[0]);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string dest = Path.Combine(extractPath, name);
            File.Copy(file, dest, true);
        }

        //Remove temp folder
        Directory.Delete(dirs[0], true);

        Console.WriteLine("Installed update succesfully to " + extractPath);
        Process.Start(Path.Combine(extractPath, "AviDrugZ.exe"));
        Environment.Exit(0);
    }

    public static string downloadUpdate(string downloadUrl)
    {


        Console.WriteLine("Downloading update from " + downloadUrl);
        string downloadPath = Path.Combine(Path.GetTempPath(), "AviDrugZ.zip");
        using (WebClient client = new WebClient())
        {
            client.DownloadFile(downloadUrl, downloadPath);
        }
        Console.WriteLine("Downloaded update succesfully to " + downloadPath);
        return downloadPath;

    }
}