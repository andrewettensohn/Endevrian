using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models.MapModels
{
    public class NewMapViewModel
    {
        public int SelectedCampaignID { get; set; }

        public List<SessionNote> SelectedCampaignSessionNotes { get; set; }

        public List<SessionSection> SelectedCampaignSessionSections { get; set; }

    }
}
