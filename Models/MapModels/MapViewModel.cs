using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models.MapModels
{
    public class MapViewModel
    {

        public Campaign SelectedCampaign { get; set; }

        public List<List<Map>> UserMaps { get; set; }

        public string UserSearchQuery { get; set; }

    }
}
