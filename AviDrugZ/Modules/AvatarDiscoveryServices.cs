using AviDrugZ.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AviDrugZ.Modules
{
    public class AvatarDiscoveryService
    {
        public ObservableCollection<AvatarModel> Avatars { get; private set; }

        private LogScanner logScanner;
        private AmplitudeCacheScanner amplitudeScanner;

        public AvatarDiscoveryService()
        {
            Avatars = new ObservableCollection<AvatarModel>();

            logScanner = new LogScanner();
            amplitudeScanner = new AmplitudeCacheScanner();

            logScanner.Avatars.CollectionChanged += (s, e) => Sync(logScanner.Avatars);
            amplitudeScanner.Avatars.CollectionChanged += (s, e) => Sync(amplitudeScanner.Avatars);
        }

        public void Start()
        {
            logScanner.Start();
            amplitudeScanner.Start();
        }

        public void Stop()
        {
            logScanner.Stop();
            amplitudeScanner.Stop();
        }

        private void Sync(ObservableCollection<AvatarModel> source)
        {
            foreach (var avatar in source)
            {
                if (!Avatars.Any(a => a.AvatarID == avatar.AvatarID))
                {
                    Avatars.Add(new AvatarModel
                    {
                        AvatarID = avatar.AvatarID,
                        CacheLocation = avatar.CacheLocation
                    });

                    Console.WriteLine($"[Unified] Added: {avatar.AvatarID} from {avatar.CacheLocation}");
                }
            }
        }
    }
}
