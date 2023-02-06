using AviDrugZ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AviDrugZ.ViewModels
{
    public class SearchViewModel : BaseModel
    {
        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value;
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
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AvatarModel> _avatarModels;

        public ObservableCollection<AvatarModel> AvatarModelsList
        {
            get { return _avatarModels; }
            set { _avatarModels = value;
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

        public void SearchForAvatars()
        {
            if (!Loading)
            {
                Loading = true;

                Task<ObservableCollection<AvatarModel>> task = null;
                //Start Task to get avatars 
                if (SelectedSearchType == SearchType.Name)
                {
                    task = WebManager.getAvatarsByName(SearchText);
                }
                else if (SelectedSearchType==SearchType.Author)
                {
                    task = WebManager.getAvatarsByAuthor(SearchText);
                }

                //Run Task asnychronosly
                task.ContinueWith((t) =>
                {
                    //Set result to AvatarModelsList
                    AvatarModelsList = t.Result;
                    Loading = false;
                });
            }
        }
    }

}
