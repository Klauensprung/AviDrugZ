using AviDrugZ.ViewModels;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using Wpf.Ui.Controls;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace AviDrugZ.Views
{
    /// <summary>
    /// Interaktionslogik für NyanLoading.xaml
    /// </summary>
    public partial class NyanLoading : UiWindow
    {
        private NyanLoadingViewModel context;
        public TextBox textBox;
        public static NyanLoading instance;
        public void closeMe()
        {
            //Invoke Close on dispatcher
            dispatcher.Invoke(() => Close());
        }

        
        
        public Dispatcher dispatcher;
        public NyanLoading()
        {
            InitializeComponent();
            this.Closing += NyanLoading_Closing;
            //Set dispatcher
            dispatcher = Dispatcher;

            instance = this;
            textBox = Output;
            //Set view model to nyanLoadingViewModel
            context = new NyanLoadingViewModel();
            this.DataContext = context;
            //Set Richbox text font to Consolas
            var font = new FontFamily("Consolas");
            textBox.FontFamily = font;


             context.startLoading();


        }

        private void NyanLoading_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            context.player.Stop();
            
        }

        public void setLoadingString(string loading)
        {
            context.LoadingString = loading;
        }

        public void addProgress(float progress)
        {
            context.Progress += progress;
        }

        public void setProgress(float progress)
        {
            context.Progress = progress;
        }
        
        public void setMaxProgress(float max)
        {
            context.MaxProgress = max;
        }

        private void UiWindow_KeyDown(object sender, KeyEventArgs e)
        {
            context.player.Stop();
        }
    }
}
