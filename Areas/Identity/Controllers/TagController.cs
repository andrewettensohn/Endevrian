using Endevrian.Controllers;
using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Models.TagModels;
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
    [Route("Identity/User/api/Tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemLogController _logController;
        private readonly QueryHelper _queryHelper;

        public TagController(ApplicationDbContext context, SystemLogController logController, IConfiguration configuration)
        {
            _context = context;
            _logController = logController;
            _queryHelper = new QueryHelper(configuration, logController);
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

            if(tagName == "" || tagName is null)
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
    }
}
