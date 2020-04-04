using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Controllers;
using Endevrian.Data;
using Endevrian.Models;
using Endevrian.Models.MapModels;
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

        public IActionResult NewMap()
        {
            return View();
        }

        public IActionResult MapGallery()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MapViewModel model = new MapViewModel();
            model.UserMaps = new List<List<Map>>();

            List<Map> allMaps = _context.Maps.Where(x => x.UserId == userId).ToList();

            //Put images  in rows of 3
            for(int i = 0; i < allMaps.Count; i += 3)
            {
                List<Map> mapRow = new List<Map>();

                if(allMaps.Count > i)
                {
                    mapRow.Add(allMaps[i]);
                }

                if(allMaps.Count > i + 1)
                {
                    mapRow.Add(allMaps[i + 1]);
                }

                if (allMaps.Count > i + 2)
                {
                    mapRow.Add(allMaps[i + 2]);
                }

                model.UserMaps.Add(mapRow);
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

            bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true && x.UserId == userId).Any();
            if(selectedNoteCheck == true)
            {
                model.SelectedNote = await _context.SessionNotes.Where(x => x.SelectedSessionNote == true && x.UserId == userId).FirstAsync();
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
