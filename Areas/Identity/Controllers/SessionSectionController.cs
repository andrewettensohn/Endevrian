using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models;
using Microsoft.AspNetCore.Mvc;


namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/User/api/SessionSection")]
    [ApiController]
    public class SessionSectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SessionSectionController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/AdventureLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SessionSection>> GetSessionSection(int id)
        {
            SessionSection sessionSection = await _context.SessionSections.FindAsync(id);

            if (sessionSection == null)
            {
                return NotFound();
            }

            return sessionSection;
        }

        //POST api/<controller>
        [HttpPost]
        public async Task<ActionResult<SessionSection>> PostSessionSection([FromBody]SessionSection sessionSection)
        {

            sessionSection.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {

                Campaign selectedCampaign = await _context.Campaigns.FindAsync(sessionSection.CampaignID);

                if (selectedCampaign.UserId == sessionSection.UserId)
                {
                    await _context.SessionSections.AddAsync(sessionSection);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetSessionSection", new { id = sessionSection.SessionSectionID }, sessionSection);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch(Exception exc)
            {
                return BadRequest();
            }

        }
    }
}
