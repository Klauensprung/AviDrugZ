using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AviDrugZ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
             StartupUri = new Uri("Views/LoginView.xaml", UriKind.Relative);
        }
    }
}
