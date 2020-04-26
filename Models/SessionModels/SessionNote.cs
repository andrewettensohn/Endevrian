using Endevrian.Models.MapModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class SessionNote
    {
        public int SessionNoteID { get; set; }

        public string UserId { get; set; }

        public int CampaignID { get; set; }

        public int SessionSectionID { get; set; }

        public string SessionNoteTitle { get; set; }

        public string SessionNoteBody { get; set; }

        public bool? SelectedSessionNote { get; set; }

    }
}
