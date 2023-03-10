using AviDrugZ.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

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
            Updater u = new Updater();

            //Start Task to check for update in updater
            Task.Run(async () =>
            {
                if (u.checkForUpdate())
                {
                    //Dialogue if a new update should be installed
                    var result = MessageBox.Show("A new update is available. Do you want to install it?", "Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        //Start Update Process update.exe in directory with current folder location as argument

                        string currentPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory());

                        //Combine path to update.exe
                        string updatePath = System.IO.Path.Combine(currentPath, "Updater.exe");

                        string oldUpdater = System.IO.Path.Combine(currentPath, "Updater_old.exe");

                        //Check if old updater exists
                        if (System.IO.File.Exists(oldUpdater))
                        {
                            //Delete old updater
                            System.IO.File.Delete(oldUpdater);
                        }

                        //Rename updater in updatePath to Updater_old.exe
                        System.IO.File.Move(updatePath, System.IO.Path.Combine(currentPath, "Updater_old.exe"));

                        //Run updater_old.exe with current path as argument
                        Process.Start(System.IO.Path.Combine(currentPath, "Updater_old.exe"), currentPath);

                        //Close AviDrugZ
                        Environment.Exit(0);
                    }
                }
            });

             StartupUri = new Uri("Views/LoginView.xaml", UriKind.Relative);
        }
    }
}
