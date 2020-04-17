using Endevrian.Models;
using Endevrian.Models.MapModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Utility
{
    public static class Utilities
    {
        public static AdventureLog NewCreateDateFormatted(AdventureLog adventureLog)
        {
            DateTime createTime = DateTime.Now;
            string stringVersion = createTime.ToString("M/d/yyyy");
            createTime = DateTime.Parse(stringVersion);

            adventureLog.DisplayLogDate = stringVersion;
            adventureLog.LogDate = createTime;

            return adventureLog;
        }

        public static DateTime NewCreateDateFormatted()
        {
            DateTime createTime = DateTime.Now;
            string stringVersion = createTime.ToString("M/d/yyyy");
            createTime = DateTime.Parse(stringVersion);

            return createTime;
        }

        public static List<List<Map>> OrderMapsForRows(List<Map> allMaps, List<List<Map>> mapListVm)
        {
            for (int i = 0; i < allMaps.Count; i += 3)
            {
                List<Map> mapRow = new List<Map>();

                if (allMaps.Count > i)
                {
                    mapRow.Add(allMaps[i]);
                }

                if (allMaps.Count > i + 1)
                {
                    mapRow.Add(allMaps[i + 1]);
                }

                if (allMaps.Count > i + 2)
                {
                    mapRow.Add(allMaps[i + 2]);
                }

                mapListVm.Add(mapRow);
            }


            return (mapListVm);
        }
    }
}
