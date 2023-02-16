using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VRChat.API.Api;

namespace AviDrugZ.Models
{
    public class AvatarModel : BaseModel
    {

        
        private BitmapImage? _aviImage;
        public BitmapImage? AviImage
        {
            get {
                if (_aviImage == null && ThumbnailUrl != "" && ThumbnailUrl != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(ThumbnailUrl, UriKind.Absolute);
                    bitmap.EndInit();

                    AviImage = bitmap;
                    OnPropertyChanged();
                    return _aviImage;
                }
                else return _aviImage;
                }
            set
            {
                _aviImage = value;
                OnPropertyChanged();
            }
        }

        private DateTime ?_dateChecked;
        public DateTime ?DateChecked
        {
            get { return _dateChecked; }
            set
            {
                _dateChecked = value;
                OnPropertyChanged();
            }
        }

        private DateTime ?_dateAdded;
        public DateTime ?DateAdded
        {
            get { return _dateAdded; }
            set
            {
                _dateAdded = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage? _fullAviImage;
        public BitmapImage? FullAviImage
        {
            get
            {
                if (_fullAviImage == null && ImageUrl != "" && ImageUrl != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(ImageUrl, UriKind.Absolute);
                    bitmap.EndInit();

                    FullAviImage = bitmap;
                    OnPropertyChanged();
                    return _fullAviImage;
                }
                else return _fullAviImage;
            }
            set
            {
                _aviImage = value;
                OnPropertyChanged();
            }
        }

        public string _imageUrl;
        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                OnPropertyChanged();
            }
        }

        private string _thumbnailUrl;

        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set
            {
                _thumbnailUrl = value;
                OnPropertyChanged();
            }
        }



        private bool _private;
        public bool Private
        {
            get { return _private; }
            set
            {
                _private = value;
                OnPropertyChanged();
            }
        }

        private string _version;

        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }


        private string _avatarName;
        public string AvatarName
        {
            get { return _avatarName; }
            set
            {
                _avatarName = value;
                OnPropertyChanged();
            }
        }

        private string _authorId;

        public string AuthorId
        {
            get { return _authorId; }
            set
            {
                _authorId = value;
                OnPropertyChanged();
            }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _authorName;

        public string AuthorName
        {
            get { return _authorName; }
            set
            {
                _authorName = value;
                OnPropertyChanged();
            }
        }

        private string _avatarID;

        public string AvatarID
        {
            get { return _avatarID; }
            set
            {
                _avatarID = value;
                OnPropertyChanged();
            }
        }

        private bool _questSupported;
        public bool QuestSupported
        {
            get { return _questSupported; }
            set
            {
                _questSupported = value;
                OnPropertyChanged();
            }
        }

        private bool _isPrivate;

        public bool IsPrivate
        {
            get { return _isPrivate; }
            set
            {
                _isPrivate = value;
                OnPropertyChanged();
            }
        }


    }
}
