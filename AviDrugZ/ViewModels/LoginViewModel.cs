using aviDrug;
using AviDrugZ.Models;
using AviDrugZ.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Dpi;
using static aviDrug.VRCApiController;

namespace AviDrugZ.ViewModels
{
    public class LoginViewModel : BaseModel
    {
        VRCApiController api = new VRCApiController();
        private string _username;
        private string _password;
        private string _twoFactorCode;
        private string _errorNoticeText;

        private bool _lockedInputWaiting2FA = false;

        public bool LockedInputWaiting2FA
        {
            get { return _lockedInputWaiting2FA; }
            set
            {
                _lockedInputWaiting2FA = value;
                ShowNotice = value ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        public string ErrorNoticeText
        {
            get { return _errorNoticeText; }
            set { _errorNoticeText = value;
                  OnPropertyChanged();
            }
        }


        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string TwoFactorCode
        {
            get { return _twoFactorCode; }
            set
            {
                _twoFactorCode = value;
                OnPropertyChanged();
            }
        }
        private Visibility _showNotice = Visibility.Hidden;

        public Visibility ShowNotice
        {
            get { return _showNotice; }
            set
            {
                _showNotice = value;
                OnPropertyChanged();
            }
        }




        public bool Login()
        {

            if (!LockedInputWaiting2FA)
            {      
                loginStatus loginStatus = api.login(Username, Password);

                if (loginStatus == loginStatus.LoggedIn)
                {
                    AvatarSearchView view = new AvatarSearchView(true);
                    view.Show();
                }
                else if (loginStatus == loginStatus.Requires2FA)
                {
                    ErrorNoticeText = "Please enter your 2FA code provided by mail";
                    LockedInputWaiting2FA = true;
                }
                else if(loginStatus==loginStatus.RequiresPhone2FA)
                {
                    ErrorNoticeText = "Please enter your authenticator code";
                    LockedInputWaiting2FA = true;
                }
            }
            else
            {
             
                loginStatus loginStatus = api.finish2FAAuth(TwoFactorCode);
                if(loginStatus == loginStatus.Sucess2FA)
                {
                    AvatarSearchView view = new AvatarSearchView(true);
                    
                    view.Show();
                    return true;
                }
                else
                {
                    LockedInputWaiting2FA = false;
                  
                }
            }

            return false;


        }

        public void SkipLogin()
        {
            AvatarSearchView view = new AvatarSearchView(false);
            view.Show();
        }
    }
}
