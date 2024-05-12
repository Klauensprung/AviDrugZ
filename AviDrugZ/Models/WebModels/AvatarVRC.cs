using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviDrugZ.Models.WebModels
{
    // Using Newtonsoft.Json attributes for customization, but you can use System.Text.Json attributes as well.
    public class AvatarVRC
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }

        // Nullable for cases where assetUrl might be null in the JSON.
        public string AssetUrl { get; set; }

        public string ImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string Version { get; set; }
        public string UnityVersion { get; set; }

        // Assuming SupportedPlatforms is a string that might represent an integer or other data, so we keep it as string.
        public string SupportedPlatforms { get; set; }
    }
}
