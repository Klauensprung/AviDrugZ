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

namespace AviDrugZ.Views
{
    /// <summary>
    /// Interaktionslogik für AvatarSearchView.xaml
    /// </summary>
    public partial class AvatarSearchView : Window
    {
        public AvatarSearchView()
        {
            InitializeComponent();
            this.DataContext = new SearchViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).SearchForAvatars();
        }
    }
}
