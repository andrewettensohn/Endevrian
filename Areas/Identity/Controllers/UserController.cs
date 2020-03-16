using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Controllers;
using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Models.SessionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly SystemLogController _logger;
        private readonly ApplicationDbContext _context;
        private readonly QueryHelper _queryHelper;


        public UserController(ApplicationDbContext context, IConfiguration configuration, SystemLogController logger)
        {
            _logger = logger;
            _context = context;
            _queryHelper = new QueryHelper(configuration, logger);
        }

        public IActionResult AdventureLog()
        {

            AdventureLogViewModel model = new AdventureLogViewModel();

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign selectedCampaign = _queryHelper.ActiveCampaignQuery(userId);
            //Campaign selectedCampaign = _context.Campaigns.First(x => x.UserId == userId);
            model.SelectedCampaign = selectedCampaign;

            if (selectedCampaign.IsSelectedCampaign == true)
            {
                List<AdventureLog> adventureLogList = _context.AdventureLogs.Where(x => x.CampaignID == selectedCampaign.CampaignID).ToList();
                adventureLogList = adventureLogList.OrderByDescending(x => x.AdventureLogID).ToList();

                model.AdventureLogs = adventureLogList;

            }

            return View(model);

        }

        public async Task<IActionResult> CampaignList()
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            CampaignViewModel model = new CampaignViewModel
            {
                Campaigns = await _context.Campaigns.Where(x => x.UserId == userId).ToListAsync()
            };

            Campaign SelectedCampaign = _queryHelper.ActiveCampaignQuery(userId);

            if (SelectedCampaign.IsSelectedCampaign == true)
            {
                model.SelectedCampaign = SelectedCampaign;
            }


            return View(model);
        }

        public async Task<IActionResult> SessionNotes()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SessionPlanViewModel model = new SessionPlanViewModel
            {
                SelectedCampaign = _queryHelper.ActiveCampaignQuery(userId),
            };

            model.SessionSections = await _context.SessionSections.Where(x => x.CampaignID == model.SelectedCampaign.CampaignID).ToListAsync();
            model.SessionNotes = await _context.SessionNotes.Where(x => x.CampaignID == model.SelectedCampaign.CampaignID).ToListAsync();

            bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).Any();
            if(selectedNoteCheck == true)
            {
                model.SelectedNote = await _context.SessionNotes.Where(x => x.SelectedSessionNote == true).FirstAsync();
            }
            else
            {
                model.SelectedNote = null;
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
