using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Endevrian.Data;
using Endevrian.Models;
using System.Security.Claims;
using Endevrian.Utility;

namespace Endevrian.Controllers
{
    [Area("Identity")]
    [Route("Identity/Author/api/AdventureLogs")]
    [ApiController]
    public class AdventureLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemLogController _logController;

        public AdventureLogsController(ApplicationDbContext context, SystemLogController logController)
        {
            _context = context;
            _logController = logController;
        }

        // GET: api/AdventureLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdventureLog>> GetAdventureLog(int id)
        {
            var adventureLog = await _context.AdventureLogs.FindAsync(id);

            if (adventureLog == null)
            {
                return NotFound();
            }

            return adventureLog;
        }

        // PUT: api/AdventureLogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdventureLog(int id, AdventureLog sentAdventureLog)
        {
            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            AdventureLog adventureLog = await _context.AdventureLogs.FindAsync(id);

            if (id != sentAdventureLog.AdventureLogID || adventureLog.UserId != requestingUser)
            {
                return BadRequest();
            }
            if (!string.IsNullOrWhiteSpace(sentAdventureLog.LogTitle))
            {
                adventureLog.LogTitle = sentAdventureLog.LogTitle;
            }
            if (!string.IsNullOrWhiteSpace(sentAdventureLog.LogBody))
            {
                adventureLog.LogBody = sentAdventureLog.LogBody;
            }
            
            _context.Entry(adventureLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdventureLogExists(id))
                {
                    _logController.AddSystemLog($"WARNING: User {requestingUser} has caused a DbUpdateConcurrencyException");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AdventureLogs
        [HttpPost]
        public async Task<ActionResult<AdventureLog>> PostAdventureLog([FromBody]AdventureLog adventureLog)
        {
            try
            {
                adventureLog.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(adventureLog.LogTitle))
                {
                    adventureLog.LogTitle = "Untitled";
                }
                if (string.IsNullOrWhiteSpace(adventureLog.LogBody))
                {
                    adventureLog.LogBody = "Nothing seems to be here!";
                }

                adventureLog = Utilities.NewCreateDateFormatted(adventureLog);

                await _context.AdventureLogs.AddAsync(adventureLog);
                _context.SaveChanges();

                return CreatedAtAction("GetAdventureLog", new { id = adventureLog.AdventureLogID }, adventureLog);
            }
            catch (Exception exc)
            {
                _logController.AddSystemLog($"ERROR: {exc.Message}");
                return BadRequest();
            }
        }

        // DELETE: api/AdventureLogs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AdventureLog>> DeleteAdventureLog(int id)
        {
            AdventureLog adventureLog = await _context.AdventureLogs.FindAsync(id);

            if (adventureLog == null || adventureLog.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            _context.AdventureLogs.Remove(adventureLog);
            await _context.SaveChangesAsync();

            return adventureLog;
        }

        private bool AdventureLogExists(int id)
        {
            return _context.AdventureLogs.Any(e => e.AdventureLogID == id);
        }
    }
}
