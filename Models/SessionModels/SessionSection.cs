using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class SessionSection
    {
        public int SessionSectionID { get; set; }

        public string UserId { get; set; }

        public int CampaignID { get; set; }

        public string SessionSectionName { get; set; }

    }
}
