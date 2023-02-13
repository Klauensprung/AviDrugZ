using aviDrug;
using AviDrugZ.Models;
using AviDrugZ.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
