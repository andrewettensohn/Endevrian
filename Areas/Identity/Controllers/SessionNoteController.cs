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

        [HttpPut("EditNote")]
        public async Task<ActionResult<SessionNote>> EditSessionNote([FromBody]SessionNote sentSessionNote)
        {

            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            SessionNote sessionNote = await _context.SessionNotes.FindAsync(sentSessionNote.SessionNoteID);

            if(requestingUser != sessionNote.UserId)
            {
                return BadRequest();
            }

            sessionNote.SessionNoteBody = sentSessionNote.SessionNoteBody;

            if(sentSessionNote.SessionNoteTitle != null)
            {
                sessionNote.SessionNoteTitle = sentSessionNote.SessionNoteTitle;
            }
            else
            {
                sessionNote.SessionNoteTitle = "New Note";
            }

            _context.Entry(sessionNote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionNoteExists(sessionNote.SessionNoteID))
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
                    //bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).Any();

                    //if (selectedNoteCheck is false) { sessionNote.SelectedSessionNote = true; }

                    bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).Any();

                    if (selectedNoteCheck == true)
                    {
                        List<SessionNote> selectedNotes = _context.SessionNotes.Where(x => x.UserId == sessionNote.UserId && x.SelectedSessionNote == true).ToList();

                        foreach (SessionNote selectedNote in selectedNotes)
                        {
                            selectedNote.SelectedSessionNote = false;
                        }

                        sessionNote.SelectedSessionNote = true;
                    }
                    else
                    {
                        sessionNote.SelectedSessionNote = true;
                    }

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

        [HttpPut("{id}")]
        public async Task<IActionResult> SetSelectedSessionNote(int id)
        {

            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SessionNote sessionNoteToSelect = await _context.SessionNotes.FindAsync(id);

            if(sessionNoteToSelect.UserId == currentUser)
            {

                bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).Any();

                if (selectedNoteCheck == true)
                {
                    List<SessionNote> selectedNotes = _context.SessionNotes.Where(x => x.UserId == currentUser && x.SelectedSessionNote == true).ToList();

                    foreach (SessionNote selectedNote in selectedNotes)
                    {
                        selectedNote.SelectedSessionNote = false;
                    }

                    sessionNoteToSelect.SelectedSessionNote = true;
                }
                else
                {
                    sessionNoteToSelect.SelectedSessionNote = true;
                }

                _context.Entry(sessionNoteToSelect).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionNoteExists(id))
                    {
                        //_logger.AddSystemLog($"WARNING: User {requestingUser} has caused a DbUpdateConcurrencyException");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SessionNote>> DeleteSessionNote(int id)
        {
            SessionNote sessionNote = await _context.SessionNotes.FindAsync(id);

            if (sessionNote == null || sessionNote.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            _context.SessionNotes.Remove(sessionNote);
            await _context.SaveChangesAsync();

            return sessionNote;
        }

        private bool SessionNoteExists(int id)
        {
            return _context.SessionNotes.Any(e => e.SessionNoteID == id);
        }
    }
}
