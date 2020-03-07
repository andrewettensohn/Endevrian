using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class CampaignViewModel
    {
        public List<Campaign> Campaigns { get; set; }

        public Campaign SelectedCampaign { get; set; }

    }
}
