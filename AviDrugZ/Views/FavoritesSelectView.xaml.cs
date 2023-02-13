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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace AviDrugZ.Views
{
    /// <summary>
    /// Interaktionslogik für FavoritesSelectView.xaml
    /// </summary>
    public partial class FavoritesSelectView : Window
    {
        public FavoritesSelectView()
        {
            
            InitializeComponent();
            //this.Width=200;
           // this.Height = 200;
        }

        private string _selectedList = "avatars1";

        public string SelectedList
        {
            get { return _selectedList; }
            set { _selectedList = value;}
        }

        public void Add_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            this.Close();

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedList = "avatars2";
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            SelectedList = "avatars3";
        }

        private void RadioButton_Click_2(object sender, RoutedEventArgs e)
        {
            SelectedList = "avatars4";
        }

        private void RadioButton_Click_3(object sender, RoutedEventArgs e)
        {
            SelectedList = "avatars5";
        }

        private void RadioButton_Click_4(object sender, RoutedEventArgs e)
        {
            SelectedList = "avatars6";
        }

        private void RadioButton_Click_5(object sender, RoutedEventArgs e)
        {
            SelectedList = "avatars1";
        }
    }
}
