using AviDrugZ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using AviDrugZ.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Controls;
using aviDrug;

namespace AviDrugZ.Views
{
    /// <summary>
    /// Interaktionslogik für AvatarSearchView.xaml
    /// </summary>
    public partial class AvatarSearchView : UiWindow
    {
        public AvatarSearchView(bool loggedIn)
        {
            InitializeComponent();
            this.DataContext = new SearchViewModel();

            if (loggedIn)
            {
                ((SearchViewModel)DataContext).VrcLoggedIn = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).SearchForAvatars(false);
        }
        
        public void SearchAvatar_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).SearchForAvatars(true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).FavoriteAvatar();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).WearAvatar();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).CopyClipboard();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ((SearchViewModel)DataContext).SearchForAvatars(false);
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //Scan cache
            ((SearchViewModel)DataContext).ScanCache();
        }
    }
}
