using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Models.MapModels;
using Endevrian.Models.TagModels;
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

        public static List<Map> GetMapGallery(string userId, string searchString, ApplicationDbContext _context)
        {
            int selectedCampaignID = _context.Campaigns.Where(x => x.UserId == userId).FirstOrDefault().CampaignID;

            List<Map> campaignMapsNoTags = _context.Maps.Where(x => x.UserId == userId && x.CampaignID == selectedCampaignID).ToList();
            List<Map> campaignMaps = new List<Map>();

            foreach (Map map in campaignMapsNoTags)
            {
                map.ActiveTags = _context.TagRelations.Where(x => x.MapID == map.MapID).ToList();
                map.InactiveTags = GetInactiveTagsForMap(map, _context);

                campaignMaps.Add(map);
            }

            List<Map> requestedMaps = campaignMaps.Where(x => x.MapName.ToLower().Contains(searchString.ToLower()) ||
            x.ActiveTags.Any(x => x.TagName.ToLower().Contains(searchString.ToLower()))).ToList();

            return requestedMaps;
        }

        public static List<Tag> GetInactiveTagsForMap(Map map, ApplicationDbContext _context)
        {
            List<Tag> InactiveTags = new List<Tag>();
            List<Tag> allTags = _context.Tags.Where(x => x.UserId == map.UserId).ToList();
            foreach (Tag tag in allTags)
            {
                List<TagRelation> matchingTags = map.ActiveTags.Where(x => x.TagID == tag.TagID).ToList();

                if (!matchingTags.Any())
                {
                    InactiveTags.Add(tag);
                }
            }

            return InactiveTags;
        }
    }
}
