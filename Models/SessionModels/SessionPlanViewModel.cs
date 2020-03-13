using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models.SessionModels
{
    public class SessionPlanViewModel
    {
        public List<SessionSection> SessionSections { get; set; }

        public List<SessionNote> SessionNotes { get; set; }

        public Campaign SelectedCampaign { get; set; }

    }
}
