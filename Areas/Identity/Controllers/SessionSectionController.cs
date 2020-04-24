using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<ActionResult<List<SessionSection>>> GetAllSessionSections()
        {
            List<SessionSection> sessionSections = await _context.SessionSections.ToListAsync();

            return sessionSections;
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

        [HttpPut("{id}")]
        public async Task<ActionResult<SessionSection>> PutSessionSection(int id, SessionSection sentSessionSection)
        {

            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            SessionSection sessionSection = await _context.SessionSections.FindAsync(id);

            if (id != sentSessionSection.SessionSectionID || sessionSection.UserId != requestingUser)
            {
                return BadRequest();
            }
            if (sentSessionSection.SessionSectionName != null)
            {
                sessionSection.SessionSectionName = sentSessionSection.SessionSectionName;
            }

            _context.Entry(sessionSection).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionSectionExists(id))
                {
                    //_logController.AddSystemLog($"WARNING: User {requestingUser} has caused a DbUpdateConcurrencyException");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //POST api/<controller>
        [HttpPost]
        public async Task<ActionResult<SessionSection>> PostSessionSection([FromBody]SessionSection sessionSection)
        {

            sessionSection.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(sessionSection.SessionSectionName == "" || sessionSection is null)
            {
                sessionSection.SessionSectionName = "Section";
            }

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

        [HttpDelete("{id}")]
        public async Task<ActionResult<SessionSection>> DeleteSessionSection(int id)
        {
            string requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SessionSection sessionSection = await _context.SessionSections.FindAsync(id);

            if(sessionSection.UserId != requestingUserId)
            {
                return BadRequest();
            }

            _context.SessionSections.Remove(sessionSection);
            await _context.SaveChangesAsync();

            return NoContent();

        }

        private bool SessionSectionExists(int id)
        {
            return _context.SessionSections.Any(e => e.SessionSectionID == id);
        }
    }
}
