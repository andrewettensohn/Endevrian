using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Endevrian.Data;
using Endevrian.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Endevrian.Utility;
using Microsoft.Extensions.Configuration;

namespace Endevrian.Controllers
{
    [Route("Home/api/AdventureLogs")]
    [ApiController]
    public class AdventureLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemLogController _logController;
        private readonly QueryHelper _queryHelper;

        public AdventureLogsController(ApplicationDbContext context, SystemLogController logController, IConfiguration configuration)
        {
            _context = context;
            _logController = logController;
            _queryHelper = new QueryHelper(configuration, logController);
        }

        // GET: api/AdventureLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdventureLog>>> GetAdventureLogs()
        {
            return (await _context.AdventureLogs.ToListAsync());
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

            if (id != sentAdventureLog.AdventureLogID || adventureLog.UserID != requestingUser)
            {
                return BadRequest();
            }
            if (sentAdventureLog.LogTitle != null)
            {
                adventureLog.LogTitle = sentAdventureLog.LogTitle;
            }
            if (sentAdventureLog.LogBody != null)
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
        public async Task<ActionResult<AdventureLog>> PostAdventureLog(AdventureLog adventureLog)
        {
            try
            {
                adventureLog.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (adventureLog.LogTitle == "" || adventureLog.LogTitle is null)
                {
                    adventureLog.LogTitle = "Untitled";
                }

                if (adventureLog.LogBody == "" || adventureLog.LogBody is null)
                {
                    adventureLog.LogBody = "Nothing seems to be here! Click here to edit.";
                }

                adventureLog.LogDate = Utilites.NewCreateDateFormatted();

                _context.AdventureLogs.Add(adventureLog);
                await _context.SaveChangesAsync();
                AddToHistoricalAdventureLogCount();

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
            var adventureLog = await _context.AdventureLogs.FindAsync(id);

            if (adventureLog == null || adventureLog.UserID != User.FindFirstValue(ClaimTypes.NameIdentifier))
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

        private async void AddToHistoricalAdventureLogCount()
        {
            List<HistoricalAdventureLogCount> logCounts = await _context.HistoricalAdventureLogCounts.ToListAsync();

            if (logCounts.Count() < 1)
            {
                HistoricalAdventureLogCount logCount = new HistoricalAdventureLogCount
                {
                    HistoricalLogCount = 1
                };

                try
                {
                    await _context.HistoricalAdventureLogCounts.AddAsync(logCount);
                    await _context.SaveChangesAsync();
                    _logController.AddSystemLog("INFO: Created New Row In HistoricalAdventureLogCounts table.");
                }
                catch(Exception exc)
                {
                    _logController.AddSystemLog($"ERROR: Unable to Create New Row In HistoricalAdventureLogCounts table: {exc}");
                }

            }
            else if (logCounts.Count() == 1)
            {

                HistoricalAdventureLogCount logCount = logCounts.First();
                logCount.HistoricalLogCount++;
                _queryHelper.UpdateQuery($"UPDATE HistoricalAdventureLogCounts SET HistoricalLogCount = {logCount.HistoricalLogCount} WHERE HistoricalAdventureLogCountID = 1");

            }
            else
            {
                _logController.AddSystemLog("WARNING: There is more than one row in the HistoricalAdventureLogCounts table");
            }

            return;
        }
    }
}
