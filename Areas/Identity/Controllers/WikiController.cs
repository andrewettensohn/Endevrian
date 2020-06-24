using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models.WikiModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/Author/api/Tag")]
    [ApiController]
    public class WikiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WikiController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        // POST: api/Tag
        [HttpPost("{tagName}")]
        public async Task<ActionResult<WikiPage>> PostWikiPage(WikiPage sentWikiPage)
        {

            
            if (sentWikiPage.WikiPageID != 0)
            {
                //Update Existing Wiki Page

                WikiPage currentWikiPage = await _context.WikiPages.FindAsync(sentWikiPage.WikiPageID);
                
                if(currentWikiPage.UserId != sentWikiPage.UserId)
                {
                    return BadRequest();
                }

                if(currentWikiPage.ImagePath is null)
                {

                }

                _context.Entry(sentWikiPage).State = EntityState.Modified;
            }
            else
            {
                //Create New Wiki Page
                await _context.AddAsync();

            }

            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);


            await _context.SaveChangesAsync();

            return tag;
        }

        private async Task<IActionResult> UploadWikiImage(WikiPage wikiPage)
        {

            return Ok();
        }
    }
}
