﻿using Endevrian.Controllers;
using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Models.MapModels;
using Endevrian.Models.TagModels;
using Endevrian.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Endevrian.Areas.Identity.Controllers
{

    [Area("Identity")]
    [Route("Identity/Author/api/Tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemLogController _logController;

        public TagController(ApplicationDbContext context, SystemLogController logController, IConfiguration configuration)
        {
            _context = context;
            _logController = logController;
        }

        // GET: api/Tags
        [HttpGet("Tags")]
        public async Task<ActionResult<IEnumerable<AdventureLog>>> GetAdventureLogs()
        {
            return await _context.AdventureLogs.ToListAsync();
        }

        // POST: api/Tag
        [HttpPost("{tagName}")]
        public async Task<ActionResult<Tag>> PostTag(string tagName)
        {

            if (tagName == "" || tagName is null)
            {
                tagName = "New Tag";
            }

            Tag tag = new Tag
            {
                Name = tagName,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _context.AddAsync(tag);
            await _context.SaveChangesAsync();

            return tag;
        }

        [HttpPut]
        public async Task<ActionResult<Tag>> UpdateTagName([FromBody]Tag sentTag)
        {
            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Tag tag = await _context.Tags.FindAsync(sentTag.TagID);

            if (tag.UserId != requestingUser)
            {
                return BadRequest();
            }

            if(sentTag.Name == "" || sentTag.Name is null)
            {
                tag.Name = "New Tag";
            }
            else
            {
                tag.Name = sentTag.Name;
            }

            _context.Entry(tag).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return tag;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Tag tagToDelete = await _context.Tags.FindAsync(id);

            if(tagToDelete.UserId != requestingUser)
            {
                return BadRequest();
            }

            List<Map> allUserMaps = Utilities.GetMapGallery(tagToDelete.UserId, "", _context);
            
            foreach(Map map in allUserMaps)
            {
                if(map.ActiveTags.Any(x => x.TagID == tagToDelete.TagID))
                {
                    int tagRelationID = map.ActiveTags.FirstOrDefault(x => x.TagID == tagToDelete.TagID).TagRelationID;
                    TagRelation tagRelation = await _context.TagRelations.FindAsync(tagRelationID);
                    _context.TagRelations.Remove(tagRelation);
                }
            }

            _context.Tags.Remove(tagToDelete);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost("Relate")]
        public async Task<ActionResult<TagRelation>> PostTagRelation([FromBody]TagRelation relation)
        {
            relation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Map map = await _context.Maps.FindAsync(relation.MapID);
            Tag tag = await _context.Tags.FindAsync(relation.TagID);

            if(map is null || map.UserId != relation.UserId || tag.UserId != relation.UserId || tag is null)
            {
                return BadRequest();
            }

            relation.MapName = map.MapName;
            relation.TagName = tag.Name;

            await _context.AddAsync(relation);
            await _context.SaveChangesAsync();

            return relation;
        }

        [HttpDelete("Relate/{id}")]
        public async Task<IActionResult> DeleteTagRelation(int id)
        {
            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            TagRelation tagRelation = await _context.TagRelations.FindAsync(id);

            if(tagRelation.UserId != requestingUser)
            {
                return BadRequest();
            }

            _context.TagRelations.Remove(tagRelation);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
