using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviDrugZ.Models.WebModels
{
    internal class AvatarWeb
    {
        public int entry { get; set; }
        public string Id { get; set; }
        public string DateAdded { get; set; }
        public string LastChecked { get; set; }
        public string AvatarName { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public string? AssetUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public int IsPrivate { get; set; }
        public int SupportedPlatforms { get; set; }
        public int? Version { get; set; }
        public int IsDeleted { get; set; }
        public string UnityVersion { get; set; }
    }
}
