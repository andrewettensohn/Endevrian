﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class AdventureLog
    {
        public int AdventureLogID { get; set; }

        public string UserId { get; set; }

        public int CampaignID { get; set; }

        public string LogTitle { get; set; }

        public string LogBody { get; set; }

        public string DisplayLogDate { get; set; }

        public DateTime LogDate { get; set; }

    }
}
