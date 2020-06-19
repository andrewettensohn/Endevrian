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

        public bool LogsFound { get; set; }

    }
}
