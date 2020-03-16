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
    [Route("Identity/User/api/SessionNote")]
    [ApiController]
    public class SessionNoteController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public SessionNoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SessionNote>> GetSessionNoteAsync(int id)
        {

            SessionNote sessionNote = await _context.SessionNotes.FindAsync(id);

            return sessionNote;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<ActionResult<SessionNote>> PostSessionNote([FromBody]SessionNote sessionNote)
        {

            sessionNote.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(sessionNote.SessionNoteTitle == ""){ sessionNote.SessionNoteTitle = "New Note"; }

            try
            {

                Campaign selectedCampaign = await _context.Campaigns.FindAsync(sessionNote.CampaignID);

                if (selectedCampaign.UserId == sessionNote.UserId)
                {
                    //SessionNote selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).First();
                    bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).Any();

                    if (selectedNoteCheck is false) { sessionNote.SelectedSessionNote = true; }

                    await _context.SessionNotes.AddAsync(sessionNote);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetSessionNote", new { id = sessionNote.SessionNoteID }, sessionNote);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return BadRequest();
            }
        }

        //[HttpPut]
        //public async Task<IActionResult>
    }
}
