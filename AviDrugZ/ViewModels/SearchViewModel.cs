using aviDrug;
using AviDrugZ.Models;
using AviDrugZ.Models.WebModels;
using AviDrugZ.Modules;
using AviDrugZ.Views;
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
using VRChat.API.Model;

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
            set { _searchText = value;
                OnPropertyChanged();
            }
        }

        private string _avatarCount = "";

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
            get
            {
                return Enum.GetValues(typeof(SearchType))
                    .Cast<SearchType>();
            }
        }



        private bool _loading = false;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                
                if (_loading)
                {
                    ShowLoading = Visibility.Visible;
                }
                else
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
            set { _avatarModels = value;
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

        public void ScanCache()
        {


            if (MessageBox.Show("Would you like to scan your cache ? This may take a minute or two", "Scan", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
             
                
            if (!VrcLoggedIn)
            {
                MessageBox.Show("Please login to VRChat first/restart the program");
                return;
            }

            if (!Loading)
            {
                Loading = true;
                CacheScanner scanner = new CacheScanner();

                //Run scanCacheFast Task and continue with 
                string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string cachePath = $"{cacheFolder}\\..\\LocalLow\\VRChat\\VRChat\\Cache-WindowsPlayer";

                //FOLDER BROWSER DIALOG WHERE WPF ?? WHERE ?? WTF IS WRONG WITH THIS FRAMEWORK
                
                Task<List<string>> task = Task.Run(()=>scanner.scanCacheFast(-1, cachePath));

                task.ContinueWith((t) =>
                {
                    List<AvatarWeb> webModels = new List<AvatarWeb>();
                    ObservableCollection<AvatarModel> avatarModels = new ObservableCollection<AvatarModel>();
                    foreach (string item in task.Result)
                    {
                        try
                        {
                            Avatar a = VRCApiController.instance.AvatarApi.GetAvatar(item);
                            AvatarModel model = new AvatarModel();
                            AvatarWeb webmodel = new AvatarWeb();
                            
                            model.AvatarID = a.Id;
                            model.AvatarName = a.Name;
                            model.AuthorName = a.AuthorName;
                            model.AuthorId = a.AuthorId;
                            model.DateAdded = a.CreatedAt;
                            model.Description = a.Description;
                            model.ThumbnailUrl = a.ThumbnailImageUrl;
                            model.ImageUrl = a.ImageUrl;
                            model.Version = a.UnityPackages[0].UnityVersion;
                            model.DateChecked = DateTime.Today;

                            webmodel.AuthorId = a.AuthorId;
                            webmodel.AuthorName = a.AuthorName;
                            webmodel.AvatarName = a.Name;
                            webmodel.AssetUrl = "idk";
                            webmodel.SupportedPlatforms = 1;
                            webmodel.IsPrivate = 0;
                            webmodel.DateAdded = DateTime.Today.ToString();
                            webmodel.LastChecked = DateTime.Today.ToString();
                            webmodel.Description = a.Description;

                            string latestVersion = "";

                            foreach (UnityPackage package in a.UnityPackages)
                            {
                                if (package.UnityVersion.CompareTo(latestVersion) > 0)
                                {
                                    latestVersion = package.UnityVersion;
                                }

                                if (package.Platform == "android")
                                {
                                    model.QuestSupported = true;
                                    webmodel.SupportedPlatforms = 3;
                                }
                            }

                            webmodel.UnityVersion = latestVersion;
                            webmodel.Version = a._Version;

                            avatarModels.Add(model);
                        }
                        catch (Exception e)
                        {
                            
                        }

                        
                    }
                    

                    //Set result to AvatarModelsList
                    //Order Task result by dateAdded
                    AvatarModelsList = avatarModels;
                    Loading = false;
                    
                    KlauenUtils.exportToKlauentec(webModels);
                });

                
            }
            else
            {
                MessageBox.Show("Please wait until the current scan is finished!");
                return;
            }

        }


        public async void CopyClipboard()
        {
            if (SelectedAvatar == null)
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
            if (SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }
            
            List<string> toUpdateOnlineList = new List<string>();
            

            FavoritesSelectView favoriteWindow = new FavoritesSelectView();
            
            
            favoriteWindow.ShowDialog();
            string result = favoriteWindow.SelectedList;
            toUpdateOnlineList.Add(result);
            if (favoriteWindow.DialogResult == true)
            {


                AddFavoriteRequest request = new AddFavoriteRequest(FavoriteType.Avatar, SelectedAvatar.AvatarID, toUpdateOnlineList);
                try
                {
                    VRCApiController.instance.FavoritesApi.AddFavorite(request);
                    MessageBox.Show("Sucessfully favorited avatar!");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Couldn't favorite avatar due to " + e.Message);
                    //throw;
                }
            }


        }

        private void FavoriteWindow_DialogDone(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void WearAvatar()
        {
            if (SelectedAvatar == null)
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }
            
            VRCApiController.instance.AvatarApi.SelectAvatar(SelectedAvatar.AvatarID);
            MessageBox.Show("Sucessfully worn avatar!");
        }

        public void SearchForAvatars(bool author)
        {
            
            if (!Loading)
            {
                Loading = true;

                Task<ObservableCollection<AvatarModel>> task = null;
                //Start Task to get avatars 

                if (author)
                {
                    if (SelectedAvatar == null) { return; }
                    task = WebManager.getAvatarsByAuthor(SelectedAvatar.AuthorName);
                }
                else
                {
                    if (SelectedSearchType == SearchType.Name)
                    {
                        task = WebManager.getAvatarsByName(SearchText);
                    }
                    else if (SelectedSearchType == SearchType.Author)
                    {
                        task = WebManager.getAvatarsByAuthor(SearchText);
                    }
                }
                //Run Task asnychronosly
                task.ContinueWith((t) =>
                {
                    //Set result to AvatarModelsList
                    ObservableCollection<AvatarModel> taskResult = t.Result;

                    //Order Task result by dateAdded
                    taskResult = new ObservableCollection<AvatarModel>(taskResult.OrderByDescending(x => x.DateAdded));

                    AvatarModelsList = taskResult;
                    Loading = false;
                });
            }
        }
    }

}
