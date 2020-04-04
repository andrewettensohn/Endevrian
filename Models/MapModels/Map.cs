using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models.MapModels
{
    public class Map
    {
        public int MapID { get; set; }

        public int CampaignID { get; set; }

        public string UserId { get; set; }

        public string MapName { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string PreviewFileName { get; set; }

        public string PreviewFilePath { get; set; }

    }
}
