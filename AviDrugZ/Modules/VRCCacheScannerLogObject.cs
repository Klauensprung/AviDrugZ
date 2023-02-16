using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviDrugZ.Modules
{
    public class VRCCacheScannerLogObject
    {
        public string ScanDate { get; set; }

        public string FileCreationDate { get; set; }

        public string FilePath { get; set; }

        public string FileHash { get; set; }

        public string ContentType { get; set; }

        public string BundleName { get; set; }

        public string ID { get; set; }

        public VRCCacheScannerLogObject(string scandate, string filecreationdate, string filepath, string filehash, string content, string bundlename, string id)
        {
            ScanDate = scandate;
            FileCreationDate = filecreationdate;
            FilePath = filepath;
            FileHash = filehash;
            ContentType = content;
            BundleName = bundlename;
            ID = id;
        }
    }
}
