using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class AdventureLogViewModel
    {
        public List<AdventureLog> AdventureLogs { get; set; }

        public Campaign SelectedCampaign { get; set; }

        //TODO maybe just display create if there is a selected campaign, otherwise the user needs to be told there's no campaign selected.

    }
}
