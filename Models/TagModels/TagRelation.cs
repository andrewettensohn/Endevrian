﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models.TagModels
{
    public class TagRelation
    {
        public int TagRelationID { get; set; }

        public int MapID { get; set; }

        public int TagID { get; set; }

        //Activate Tag -> New Tag Relation
        //Map ActiveTags _context.TagRelation.Where(x => x.MapID == map.mapID
    }
}
