using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Endevrian.Controllers
{
    [Route("Home/api/Campaigns")]
    public class CampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SystemLogController _logger;
        private readonly QueryHelper _queryHelper;

        public CampaignController(ApplicationDbContext context, SystemLogController logController, IConfiguration configuration)
        {
            _context = context;
            _logger = logController;
            _queryHelper = new QueryHelper(configuration, logController);
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
        public async Task<ActionResult<Campaign>> PostCampaign(Campaign campaign)
        {

            try
            {
                campaign.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (campaign.CampaignName is null)
                {
                    campaign.CampaignName = "Your Campaign";
                }

                campaign.CampaignCreateDate = Utilites.NewCreateDateFormatted();

                await _context.AddAsync(campaign);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCampaign", new { id = campaign.CampaignID }, campaign);
            }
            catch(Exception exc)
            {
                _logger.AddSystemLog("Error", $"Unable to create new campaign: {exc}");
                return BadRequest();
            }

        }
    }
}
