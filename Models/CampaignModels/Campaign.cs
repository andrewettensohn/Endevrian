using Endevrian.Models.WikiModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class Campaign
    {
        public int CampaignID { get; set; }
        
        public string UserId { get; set; }

        public string CampaignName { get; set; }

        public string CampaignDescription { get; set; }

        public bool? IsSelectedCampaign { get; set; }

        public DateTime CampaignCreateDate { get; set; }

        [NotMapped]
        public List<WikiPage> WikiPages { get; set; }
    }
}
