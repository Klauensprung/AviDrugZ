using AviDrugZ.Models;
using AviDrugZ.Views;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using static log4net.Appender.ColoredConsoleAppender;

namespace AviDrugZ.ViewModels
{
    class NyanLoadingViewModel : BaseModel
    {
		private string _consoleText;

		public string ConsoleText
		{
			get { return _consoleText; }
			set { _consoleText = value;
				  OnPropertyChanged();
			}
		}

        private string _loadingString = " Scanning Cache \r\nPress any [key] to [mute] audio";

        public string LoadingString
        {
            get { return _loadingString; }
            set
            {
                _loadingString = value;
                OnPropertyChanged();
            }
        }

        private float _maxProgress;

        public float MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }

        private float _progress;

		public float Progress
		{
			get { return _progress; }
			set { _progress = value;
				OnPropertyChanged();
			}
		}

		private SolidColorBrush _color;

		public SolidColorBrush Color
		{
			get { return _color; }
			set {
                _color = value;
				OnPropertyChanged();
			}
		}


        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged();
            }
        }


        private string textPercentage()
        {
            string progress = "";

            float times = (float)((Progress / MaxProgress)*100);
            string line = "";
            for (int i = 0; i< times/2;i++)
            {
                line += "█";
            }
            
            for(int i = 0; i< 4;i++)
            {
                progress += line + "\r\n";
            }

            progress += $"{times}% {Progress}/{MaxProgress} {LoadingString} \r\n#Cracky0001";
            return progress;
        }

        public SoundPlayer player;

        public async void startLoading()
		{
            Random r = new Random();


            //Get Resource sound clip
            player = new System.Media.SoundPlayer(@"Resources\Nyan.wav");
            player.Play();

            Loading = true;

			while (Loading)
			{

                ConsoleText = @"
          ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
        ▄▀░░░░░░░░░░░░▄░░░░░░░▀▄
        █░░▄░░░░▄░░░░░░░░░░░░░░█
        █░░░░░░░░░░░░▄█▄▄░░▄░░░█ ▄▄▄
 ▄▄▄▄▄  █░░░░░░▀░░░░▀█░░▀▄░░░░░█▀▀░██
 ██▄▀██▄█░░░▄░░░░░░░██░░░░▀▀▀▀▀░░░░██
  ▀██▄▀██░░░░░░░░▀░██▀░░░░░░░░░░░░░▀██
    ▀████░▀░░░░▄░░░██░░░▄█░░░░▄░▄█░░██
       ▀█░░░░▄░░░░░██░░░░▄░░░▄░░▄░░░██
       ▄█▄░░░░░░░░░░░▀▄░░▀▀▀▀▀▀▀▀░░▄▀
      █▀▀█████████▀▀▀▀████████████▀
      ████▀  ███▀     ▀███  ▀██▀

 ███╗   ██╗██╗   ██╗ █████╗ ███╗   ██╗
 ████╗  ██║╚██╗ ██╔╝██╔══██╗████╗  ██║
 ██╔██╗ ██║ ╚████╔╝ ███████║██╔██╗ ██║
 ██║╚██╗██║  ╚██╔╝  ██╔══██║██║╚██╗██║
 ██║ ╚████║   ██║   ██║  ██║██║ ╚████║
 ╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═══╝

";


                ConsoleText += textPercentage();

            
                await Task.Delay(100);
                ConsoleText = @"
          ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
        ▄▀░░░░░░░░░░░░▄░░░░░░░▀▄
        █░░▄░░░░▄░░░░░░░░░░░░░░█
        █░░░░░░░░░░░░▄█▄▄░░▄░░░█ ▄▄▄
 ▄▄▄▄▄  █░░░░░░▀░░░░▀█░░▀▄░░░░░█▀▀░██
 ██▄▀██▄█░░░▄░░░░░░░██░░░░▀▀▀▀▀░░░░██
  ▀██▄▀██░░░░░░░░▀░██▀░░░░░░░░░░░░░▀██
    ▀████░▀░░░░▄░░░██░░░▄█░░░░▄░▄█░░██
       ▀█░░░░▄░░░░░██░░░░▄░░░▄░░▄░░░██
       ▄█▄░░░░░░░░░░░▀▄░░▀▀▀▀▀▀▀▀░░▄▀
    █▀▀█████████▀▀▀▀████████████▀▀▀
   ████▀  ███▀       ▀███  ▀██▀

 ███╗   ██╗██╗   ██╗ █████╗ ███╗   ██╗
 ████╗  ██║╚██╗ ██╔╝██╔══██╗████╗  ██║
 ██╔██╗ ██║ ╚████╔╝ ███████║██╔██╗ ██║
 ██║╚██╗██║  ╚██╔╝  ██╔══██║██║╚██╗██║
 ██║ ╚████║   ██║   ██║  ██║██║ ╚████║
 ╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═══╝

";

                ConsoleText += textPercentage();

      

                //Invoke on main thread
                //Randomize color
                await Task.Delay(100);
                

                System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
                Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(randomColor.A, randomColor.R, randomColor.G, randomColor.B));

                //Invoke consoleText change on main Thread
                //ConsoleText = "Test";
                //ConsoleText = "Test";
                //Create new Thread to changes ConsoleText


            }
        }



	}
}
