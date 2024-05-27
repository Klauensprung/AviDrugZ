using aviDrug;
using AviDrugZ.Models;
using AviDrugZ.Models.VRC;
using AviDrugZ.Models.WebModels;
using AviDrugZ.Modules;
using AviDrugZ.Views;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Polly.Caching;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VRChat.API.Model;
using static AviDrugZ.Modules.LiveScanner;

namespace AviDrugZ.ViewModels
{
    public class SearchViewModel : BaseModel
    {
        private string _searchText;

        private bool _vrcLoggedIn;

        public bool VrcLoggedIn
        {
            get { return _vrcLoggedIn; }
            set
            {
                _vrcLoggedIn = value;
                OnPropertyChanged();
            }
        }


        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private string _avatarCount = "";

        private int _avatarsNew = 0;
        public int AvatarsNew
        {
            get { return _avatarsNew; }
            set
            {
                _avatarsNew = value;
                OnPropertyChanged();
            }
        }

        public string AvatarCount
        {
            get { return _avatarCount; }
            set
            {
                _avatarCount = value;
                OnPropertyChanged();
            }
        }

        private Visibility _showLoading = Visibility.Hidden;

        public Visibility ShowLoading
        {
            get { return _showLoading; }
            set
            {
                _showLoading = value;
                OnPropertyChanged();
            }
        }

        public enum SearchType
        {
            Name,
            Author
        }


        private SearchType _selectedSearchType = SearchType.Name;

        public SearchType SelectedSearchType
        {
            get { return _selectedSearchType; }
            set
            {
                _selectedSearchType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<SearchType> SearchTypes
        {
            get { return Enum.GetValues(typeof(SearchType)).Cast<SearchType>(); }
        }


        private bool _loading = false;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;

                if(_loading)
                {
                    ShowLoading = Visibility.Visible;
                } else
                {
                    ShowLoading = Visibility.Hidden;
                }

                OnPropertyChanged();
            }
        }

        private ObservableCollection<AvatarModel> _avatarModels;

        public ObservableCollection<AvatarModel> AvatarModelsList
        {
            get { return _avatarModels; }
            set
            {
                _avatarModels = value;
                AvatarCount = _avatarModels.Count.ToString() + " avatars found";
                OnPropertyChanged();
            }
        }

        private AvatarModel _selectedAvatar;

        public AvatarModel SelectedAvatar
        {
            get { return _selectedAvatar; }
            set
            {
                _selectedAvatar = value;
                OnPropertyChanged();
            }
        }

        public void OpenAvatarFolder()
        {
            //Open cache location in new folder
            if(SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first");
                return;
            }

            if (SelectedAvatar.CacheLocation == null) return;
            //Trim last 8 letters from cacheLocations
            string directory = SelectedAvatar.CacheLocation.Substring(0, SelectedAvatar.CacheLocation.Length - 7);





            if (!Directory.Exists(directory))
            {
                MessageBox.Show("Avatar cache location not found");
                return;
            }

            //Open folder directory


            var psi = new System.Diagnostics.ProcessStartInfo() { FileName = directory, UseShellExecute = true };
            System.Diagnostics.Process.Start(psi);
      
        }

        //For non logged in users, asking directly the vrcdb endpoint for this avatars info
        public static AvatarModel ConvertToAvatarModel(string json)
        {
            try
            {
                //Parse json to List<AvatarSimple>
                AvatarSimple avatar = JsonConvert.DeserializeObject<AvatarSimple>(json);

                AvatarModel newModel = new AvatarModel();
                newModel.AvatarID = avatar.Id;
                newModel.AvatarName = avatar.AvatarName;
                newModel.AuthorName = avatar.AuthorName;
                newModel.Description = avatar.Description;
                newModel.ThumbnailUrl = avatar.ThumbnailUrl;
                newModel.ImageUrl = avatar.ThumbnailUrl;
                newModel.QuestSupported = avatar.SupportedPlatforms == 3 ? true : false;
                newModel.Version = avatar.UnityVersion;
                newModel.DateAdded = avatar.DateAdded;

                return newModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }


        public void ScanCache()
        {
            if(MessageBox.Show(
                    "Would you like to scan your cache ? This may take a minute or two",
                    "Scan",
                    MessageBoxButton.YesNo) ==
                MessageBoxResult.No)
                return;

            CacheScanning = true;

            //if(!VrcLoggedIn)
            //{
            //    MessageBox.Show("Please login to VRChat first/restart the program");
            // //   return;
            //}

          
            if(!Loading)
            {

                //abort live scan
                if(LiveScanning) abortLiveScanning();

                Loading = true;
                CacheScanner scanner = new CacheScanner();
    

                // Interlocked.Add(ref totalCount, total);

                //Run scanCacheFast Task and continue with 
                string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string cachePath = $"{cacheFolder}Low\\VRChat\\VRChat\\Cache-WindowsPlayer";
                //Lets not talk about this workaround to find localLow please kthxbye 

                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Select your VrChat Cache folder called Cache-WindowsPlayer";

                if (Directory.Exists(cachePath))
                {
                    dialog.SelectedPath = cachePath;
                }

                if (!dialog.ShowDialog().Value)
                {
                    Loading = false;
                    return;
                }
                cachePath = dialog.SelectedPath;

                NyanLoading loading = new NyanLoading();
                loading.Show();

                AvatarLogger.InitializeSession();

                Task<List<CacheResult>> task = Task.Run(() => scanner.scanCacheFast(-1, cachePath,loading));

                task.ContinueWith(
                    (t) =>
                    {
                        loading.setProgress(0);
                        loading.setMaxProgress(t.Result.Count);
                        loading.setLoadingString("Checking avatars...");
                        List <AvatarWeb> webModels = new List<AvatarWeb>();
                        ObservableCollection<AvatarModel> avatarModels = new ObservableCollection<AvatarModel>();

                        foreach(CacheResult item in task.Result)
                        {
                            illegalmarker:
                            try
                            {
                                //Get avatar info from vrcdb.bs002.de/avatars/Avatar/{avatarID}
                                string url = "https://vrcdb.bs002.de/avatars/Avatar/avtr_" + item.AvatarID;
                                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                                client.DefaultRequestHeaders.Add("User-Agent", "AviDrugZ v2");
                                string json = client.GetStringAsync(url).Result;

              

                                //if not status code 500
                                if(json.Contains("Internal Server Error"))
                                {
                                    //add dummy avatar
                                    AvatarModel unknownModel = new AvatarModel();
                                    unknownModel.CacheLocation = item.CacheLocation;
                                    unknownModel.AvatarID = item.AvatarID;
                                    unknownModel.AuthorName = "Unknown";
                                    unknownModel.AvatarName = "Unknown";
                                    unknownModel.Description = item.AvatarID;
                                    unknownModel.DateDownloaded = System.IO.File.GetLastWriteTime(item.CacheLocation);
                                    unknownModel.DateAdded = DateTime.Now;
                                    unknownModel.IsPrivate = false;

                                    //Try if we can get the avatar from vrcdb
                                    var newModel = KlauenUtils.exportToVRCDB(item.AvatarID);

                                    if(newModel != null)
                                    {
                                        AvatarModel newDisplayModel = new AvatarModel();
                                        newDisplayModel.AvatarID = newModel.Id;
                                        newDisplayModel.AvatarName = newModel.Name;
                                        newDisplayModel.AuthorName = newModel.AuthorName;
                                        newDisplayModel.Description = newModel.Description;
                                        newDisplayModel.ThumbnailUrl = newModel.ThumbnailImageUrl;
                                        newDisplayModel.ImageUrl = newModel.ImageUrl;
                                        newDisplayModel.QuestSupported = newModel.SupportedPlatforms == "3";
                                        newDisplayModel.Version = newModel.UnityVersion;
                                        newDisplayModel.DateDownloaded = System.IO.File.GetLastWriteTime(item.CacheLocation);
                                        newDisplayModel.DateAdded = DateTime.Now;
                                        newDisplayModel.IsPrivate = false;
                                        newDisplayModel.CacheLocation = item.CacheLocation;

                                        
                                        avatarModels.Add(newDisplayModel);
                                        AvatarLogger.LogAvatarModel(newDisplayModel);
                                        AvatarsNew += 1;
                                        
                                    } else
                                    {
                                        avatarModels.Add(unknownModel);
                                        AvatarLogger.LogAvatarModel(unknownModel);
                                    }

                                    loading.addProgress(1f);
                                    continue;
                                }

                                AvatarModel a = ConvertToAvatarModel(json);
                             
                                a.DateDownloaded = System.IO.File.GetLastWriteTime(item.CacheLocation);
                                a.CacheLocation = item.CacheLocation;
                                loading.addProgress(1f);

                                                                
                                
                                avatarModels.Add(a);
                                AvatarLogger.LogAvatarModel(a);

                            } catch(Exception e)
                            {

                                //is rate limited ?
                                if(e.Message.Contains("429"))
                                {
                                    Thread.Sleep(5000);
                                    goto illegalmarker;
                                }

                                //Get date added from cache write time
                                DateTime dateAdded = System.IO.File.GetLastWriteTime(item.CacheLocation);

                                //add dummy avatar
                                AvatarModel unknownModel = new AvatarModel();
                                unknownModel.CacheLocation = item.CacheLocation;
                                unknownModel.AvatarID = item.AvatarID;
                                unknownModel.AuthorName = "Unknown";
                                unknownModel.AvatarName = item.AvatarID;
                                unknownModel.Description = "Unknown";
                                unknownModel.DateAdded = dateAdded;
                                unknownModel.IsPrivate = true;
                                avatarModels.Add(unknownModel);

                                loading.addProgress(1f);
                                continue;

                             //   loading?.addProgress(1f);
                            }
                        }


                        //Set result to AvatarModelsList
                        //Order Task result by dateAdded
                        AvatarModelsList = avatarModels;
                        Loading = false;

                        loading?.closeMe();
                      
                       // export?
                    });
            } else
            {
                MessageBox.Show("Please wait until the current scan is finished!");
                return;
            }
        }


        public async void CopyClipboard()
        {
            if(SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }

            //Copy avatarID to clipboard

            DataObject data = new DataObject();

            // Add a Customer object using the type as the format.
            data.SetData(SelectedAvatar.AvatarID.GetType(), SelectedAvatar.AvatarID);


            Clipboard.SetDataObject(data);
            MessageBox.Show("Copied avatar ID to clipboard!");
        }


        public void FavoriteAvatar()
        {
            if(SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }

            List<string> toUpdateOnlineList = new List<string>();


            FavoritesSelectView favoriteWindow = new FavoritesSelectView();


            favoriteWindow.ShowDialog();
            string result = favoriteWindow.SelectedList;
            toUpdateOnlineList.Add(result);
            if(favoriteWindow.DialogResult == true)
            {
                AddFavoriteRequest request = new AddFavoriteRequest(
                    FavoriteType.Avatar,
                    SelectedAvatar.AvatarID,
                    toUpdateOnlineList);
                try
                {
                    VRCApiController.instance.FavoritesApi.AddFavorite(request);
                    MessageBox.Show("Sucessfully favorited avatar!");
                } catch(Exception e)
                {
                    MessageBox.Show("Couldn't favorite avatar due to " + e.Message);
                    //throw;
                }
            }
        }

        private void FavoriteWindow_DialogDone(object? sender, EventArgs e) { throw new NotImplementedException(); }

        public void OpenUrl(string url)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // This is crucial for URLs
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine("Failed to open URL: " + ex.Message);
            }
        }


        public void WearAvatar()
        {
            if(SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }

            try
            {
                if (!VrcLoggedIn)
                {
                    //Start browser with avatar page  safely in standard browser
                    string url = "https://vrchat.com/home/avatar/" + SelectedAvatar.AvatarID;
                    OpenUrl(url);
                
                }
                else
                {
                    //Wear avatar
                    VRCApiController.instance.AvatarApi.SelectAvatar(SelectedAvatar.AvatarID);
                    MessageBox.Show("Sucessfully worn avatar!");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            
        }

        public void GetLatestAvatars()
        {
            if (!Loading)
            {
                Loading = true;
                Task<ObservableCollection<AvatarModel>> task = WebManager.getAvatarsLatest();
                task.ContinueWith(
                        (t) =>
                        {
                            try
                            {
                                AvatarModelsList = task.Result;
                            }
                            catch (Exception)
                            {

                            }
                            Loading = false;
                        });
            }
         }

        public void abortLiveScanning()
        {
            scanner = null;
            LiveScanning = false;
        }

        public void SearchForAvatars(bool author)
        {
            if(!Loading)
            {
                Loading = true;
                CacheScanning = false;
                if (LiveScanning) abortLiveScanning();

                Task<ObservableCollection<AvatarModel>> task = null;
                //Start Task to get avatars 

                if(author)
                {
                    if(SelectedAvatar == null)
                    {
                        return;
                    }
                    task = WebManager.getAvatarsByAuthor(SelectedAvatar.AuthorName);
                } else
                {
                    if(SelectedSearchType == SearchType.Name)
                    {
                        if (SearchText == "") { Loading = false; GetLatestAvatars();return;}
                        else
                        {
                            task = WebManager.getAvatarsByName(SearchText);
                        }                     
                    } else if(SelectedSearchType == SearchType.Author)
                    {
                        task = WebManager.getAvatarsByAuthor(SearchText);
                    }
                }
                //Run Task asnychronosly
                task.ContinueWith(
                    (t) =>
                    {
                        //Set result to AvatarModelsList
                        try
                        {
                            ObservableCollection<AvatarModel> taskResult = t.Result;

                            //Order Task result by dateAdded
                            taskResult = new ObservableCollection<AvatarModel>(
                                taskResult.OrderByDescending(x => x.DateAdded));

                            AvatarModelsList = taskResult;
                        }
                        catch (Exception e)
                        {
                            Loading = false;
                            //  throw;
                        }
                        Loading = false;
                    });
            }
        }

        private bool _cacheScanning = false;

        public bool CacheScanning
        {
            get { return _cacheScanning; }
            set
            {
                _cacheScanning = value;
                OnPropertyChanged();
            }
        }

        private bool liveScanning = false;

        public bool LiveScanning
        {
            get { return liveScanning; }
            set
            {
                liveScanning = value;
                CacheScanning = value;
                OnPropertyChanged();
            }
        }

        LiveScanner scanner;

        public void ScanCacheLive()
        {
            if (!liveScanning)
            {
                AvatarModelsList = new ObservableCollection<AvatarModel>();
                LiveScanning = true;
                string cachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\VRChat\\VRChat\\Cache-WindowsPlayer";
                scanner = new LiveScanner(cachePath);

                scanner.Avatars.CollectionChanged += HandleAvatarCollectionChanged;
                AvatarLogger.InitializeSession();
            }
            else
            {
                //disable live scanning
                scanner = null;
                LiveScanning = false;
            }
        }

        int failed = 0;
        private void HandleAvatarCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                AvatarModel latestAvatar = scanner.Avatars.LastOrDefault();
                if (latestAvatar == null) return;

                string avatarID = latestAvatar.AvatarID;
                string cacheLocation = latestAvatar.CacheLocation;
                string url = $"https://vrcdb.bs002.de/avatars/Avatar/avtr_{avatarID}";

                try
                {
                    ProcessAvatar(url, cacheLocation, avatarID);
                }
                catch (Exception ex)
                {
                    failed++;
                    ProcessException(avatarID, cacheLocation);
                   
                }

                AvatarCount = AvatarModelsList.Count.ToString() + " avatars found. " + AvatarsNew + " were newly added. " + failed + " were unkown";
            }
        }

        private void ProcessAvatar(string url, string cacheLocation, string avatarID)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                string json = client.GetStringAsync(url).Result;
                if (json.Contains("Internal Server Error"))
                {
                    AddUnknownModel(avatarID, cacheLocation);
                    return;
                }

                AvatarModel avatar = ConvertToAvatarModel(json);
                avatar.DateDownloaded = System.IO.File.GetLastWriteTime(cacheLocation);
                avatar.CacheLocation = cacheLocation;
                AvatarLogger.LogAvatarModel(avatar);
                App.Current.Dispatcher.Invoke(() => AvatarModelsList.Add(avatar));
            }
        }

        private void AddUnknownModel(string avatarID, string cacheLocation)
        {
            AvatarModel unknownModel = CreateUnknownAvatarModel(avatarID, cacheLocation);
            var newModel = KlauenUtils.exportToVRCDB(avatarID);
            if (newModel != null)
            {
                AvatarModel newDisplayModel = CreateDisplayModelFromNewModel(newModel, cacheLocation);
                App.Current.Dispatcher.Invoke(() => AvatarModelsList.Add(newDisplayModel));
                AvatarLogger.LogAvatarModel(newDisplayModel);
                AvatarsNew += 1;
            }
            else
            {
                App.Current.Dispatcher.Invoke(() => AvatarModelsList.Add(unknownModel));
                AvatarLogger.LogAvatarModel(unknownModel);
            }
        }

        private void ProcessException(string avatarID, string cacheLocation)
        {
            AddUnknownModel(avatarID, cacheLocation);
        }

        private AvatarModel CreateUnknownAvatarModel(string avatarID, string cacheLocation)
        {
            return new AvatarModel
            {
                AvatarID = avatarID,
                CacheLocation = cacheLocation,
                AuthorName = "Unknown",
                AvatarName = "Unknown",
                Description = avatarID,
                DateAdded = DateTime.Now,
                DateDownloaded = System.IO.File.GetLastWriteTime(cacheLocation),
                IsPrivate = false
            };
        }

        private AvatarModel CreateDisplayModelFromNewModel(dynamic newModel, string cacheLocation)
        {
            return new AvatarModel
            {
                AvatarID = newModel.Id,
                AvatarName = newModel.Name,
                AuthorName = newModel.AuthorName,
                Description = newModel.Description,
                ThumbnailUrl = newModel.ThumbnailImageUrl,
                ImageUrl = newModel.ImageUrl,
                QuestSupported = newModel.SupportedPlatforms == "3",
                Version = newModel.UnityVersion,
                DateDownloaded = System.IO.File.GetLastWriteTime(cacheLocation)
            };
        }

        public void LoadScansFromCache()
        {
            string initialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scans");

            // Ensure the directory exists
            if (!Directory.Exists(initialPath))
            {
                Directory.CreateDirectory(initialPath); // Optionally create if it does not exist
            }

            MessageBox.Show("Navigate to the scans directory of the program and select a scan log file.");


            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = "Select a scan log file. It is located in the scans directory of the program";
            dialog.Filter = "JSON files (*.json)|*.json";
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dialog.RestoreDirectory = false;

            if (dialog.ShowDialog() == true) // Ensure dialog result is checked correctly
            {
                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    return;
                }

                string cacheLocation = dialog.FileName;
                AvatarModelsList = LiveScanner.LoadScansFromSession(cacheLocation);

                //Make new 
            }
        }

        internal void OpenInVRCX()
        {
            //run this command with the currently selected avatar id, if avatar is selected vrcx://avatar/avtr_2abf43dc-074c-489a-a20d-70e813949ffe
            if (SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }

            string url = "vrcx://avatar/" + SelectedAvatar.AvatarID;
            OpenUrl(url);


        }
    }
}
