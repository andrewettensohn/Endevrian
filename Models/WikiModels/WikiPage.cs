using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Endevrian.Models.WikiModels
{
    public class WikiPage
    {
        public int WikiPageID { get; set; }

        public string UserId { get; set; }

        public int CampaignID { get; set; }

        public string PageName { get; set; }

        public string ImagePath { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public string CardContent { get; set; }

        public string WikiContent { get; set; }
    }
}
