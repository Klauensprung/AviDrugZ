using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviDrugZ.Models.VRC
{

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class WorldJson
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public bool featured { get; set; }
            public string authorId { get; set; }
            public string authorName { get; set; }
            public int capacity { get; set; }
            public List<string> tags { get; set; }
            public string releaseStatus { get; set; }
            public string imageUrl { get; set; }
            public string thumbnailImageUrl { get; set; }
            public string @namespace { get; set; }
            public List<UnityPackage> unityPackages { get; set; }
            public int version { get; set; }
            public string organization { get; set; }
            public string previewYoutubeId { get; set; }
            public int favorites { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime publicationDate { get; set; }
            public DateTime labsPublicationDate { get; set; }
            public int visits { get; set; }
            public int popularity { get; set; }
            public int heat { get; set; }
            public int publicOccupants { get; set; }
            public int privateOccupants { get; set; }
            public int occupants { get; set; }
            public List<List<object>> instances { get; set; }
        }

        public class UnityPackage
        {
            public string id { get; set; }
            public string assetUrl { get; set; }
            public string pluginUrl { get; set; }
            public string unityVersion { get; set; }
            public object unitySortNumber { get; set; }
            public int assetVersion { get; set; }
            public string platform { get; set; }
            public DateTime created_at { get; set; }
        }


    }

