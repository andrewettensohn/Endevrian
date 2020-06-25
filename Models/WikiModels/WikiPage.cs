using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

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

        public string WikiContent { get; set; }
    }
}
