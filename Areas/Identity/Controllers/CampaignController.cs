using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Endevrian.Controllers
{
    [Area("Identity")]
    [Route("Identity/Author/api/Campaigns")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemLogController _logger;

        public CampaignController(ApplicationDbContext context, SystemLogController logController, IConfiguration configuration)
        {
            _context = context;
            _logger = logController;
        }

        [HttpGet]
        public async Task<ActionResult<Campaign>> GetCampaign(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);

            if(campaign == null)
            {
                return NotFound();
            }

            return campaign;
        }

        [HttpPost]
        public async Task<ActionResult<Campaign>> PostCampaign([FromBody]Campaign campaign)
        {

            try
            {
                campaign.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (campaign.CampaignName is null || campaign.CampaignName == "")
                {
                    campaign.CampaignName = "Your Campaign";
                }

                campaign.CampaignCreateDate = Utilities.NewCreateDateFormatted();
                campaign.CampaignDescription = "Click the edit button to make a description for this campaign.";

                await _context.AddAsync(campaign);
                await _context.SaveChangesAsync();

                await SetCampaignToSelected(campaign.CampaignID);

                return CreatedAtAction("GetCampaign", new { id = campaign.CampaignID }, campaign);
            }
            catch(Exception exc)
            {
                _logger.AddSystemLog("Error", $"Unable to create new campaign: {exc}");
                return BadRequest();
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Campaign>> DeleteCampaign(int id)
        {

            string requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign campaign = await _context.Campaigns.FindAsync(id);

            if(campaign.UserId != requestingUserId)
            {
                return BadRequest();
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> PutCampaign([FromBody]Campaign sentCampaign)
        {

            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign campaign = await _context.Campaigns.FindAsync(sentCampaign.CampaignID);

            if(requestingUser != campaign.UserId)
            {
                return BadRequest();
            }

            campaign.CampaignName = sentCampaign.CampaignName;
            campaign.CampaignDescription = sentCampaign.CampaignDescription;

            _context.Entry(campaign).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CampaignExists(campaign.CampaignID))
                {
                    _logger.AddSystemLog($"WARNING: User {requestingUser} has caused a DbUpdateConcurrencyException");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> SetCampaignToSelected(int id)
        {

            string requestingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign campaignToSelect = await _context.Campaigns.FindAsync(id);

            if(campaignToSelect.UserId != requestingUser || campaignToSelect.CampaignID != id)
            {
                return BadRequest();
            }

            List<Campaign> campaignList = await _context.Campaigns.Where(x => x.UserId == requestingUser).ToListAsync();

            foreach(Campaign campaign in campaignList)
            {
                campaign.IsSelectedCampaign = false;
                _context.Entry(campaign).State = EntityState.Modified;
                _context.SaveChanges();
            }

            campaignToSelect.IsSelectedCampaign = true;
            _context.Entry(campaignToSelect).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CampaignExists(id))
                {
                    _logger.AddSystemLog($"WARNING: User {requestingUser} has caused a DbUpdateConcurrencyException");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool CampaignExists(int id)
        {
            return _context.Campaigns.Any(e => e.CampaignID == id);
        }
    }
}
