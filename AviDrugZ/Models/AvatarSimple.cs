using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviDrugZ.Models
{
    public class AvatarSimple
    {
        
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("AvatarName")]
        public string AvatarName { get; set; }

        [JsonProperty("AuthorId")]
        public string AuthorId { get; set; }

        [JsonProperty("AuthorName")]
        public string AuthorName { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("ThumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("SupportedPlatforms")]
        public int? SupportedPlatforms { get; set; }

        [JsonProperty("DateAdded")]

        public DateTime? DateAdded { get; set; }

        [JsonProperty("Version")]
        public string? Version { get; set; }

        [JsonProperty("UnityVersion")]
        public string UnityVersion { get; set; }
    }

    public class AvatarSimpleLog : AvatarSimple
    {
        [JsonProperty("CacheLocation")]
        public string CacheLocation { get; set; }

        [JsonProperty("DateDownloaded")]
        public DateTime? DateDownloaded { get; set; }
    }
}
