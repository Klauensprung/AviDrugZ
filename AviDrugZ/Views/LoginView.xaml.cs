using aviDrug;
using AviDrugZ.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace AviDrugZ.Views
{
    /// <summary>
    /// Interaktionslogik für LoginView.xaml
    /// </summary>
    public partial class LoginView : UiWindow
    {
        public LoginView()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
           
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ((LoginViewModel)DataContext).Password = PasswordBox.Password;
            if(((LoginViewModel)DataContext).Login())
            {
                this.Close();
            }
        }

        private void Login_Skip(object sender, RoutedEventArgs e)
        {
            ((LoginViewModel)DataContext).SkipLogin();
            this.Close();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
    }
}
