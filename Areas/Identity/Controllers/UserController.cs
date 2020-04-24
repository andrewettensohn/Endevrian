﻿using System;
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
using Microsoft.Extensions.Primitives;

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

        public async Task<IActionResult> NewMap()
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign SelectedCampaign = _queryHelper.ActiveCampaignQuery(userId);

            NewMapViewModel model = new NewMapViewModel
            {
                SelectedCampaignID = SelectedCampaign.CampaignID,
                SelectedCampaignSessionSections = await _context.SessionSections.Where(x => x.CampaignID == SelectedCampaign.CampaignID && userId == x.UserId).ToListAsync(),
                SelectedCampaignSessionNotes = await _context.SessionNotes.Where(x => x.CampaignID == SelectedCampaign.CampaignID && userId == x.UserId).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> MapGallery(string searchString)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MapViewModel model = new MapViewModel
            {
                UserMaps = new List<List<Map>>(),
                SelectedCampaign = _queryHelper.ActiveCampaignQuery(userId)
            };

            if (searchString != null)
            {

                List<Map> foundMaps = _queryHelper.UserQueryMapGallery(userId, searchString);

                model.UserMaps = Utility.Utilities.OrderMapsForRows(foundMaps, model.UserMaps);

            }
            else
            {

                List<Map> allMaps = await _context.Maps.Where(x => x.UserId == userId && x.CampaignID == model.SelectedCampaign.CampaignID).ToListAsync();

                model.UserMaps = Utility.Utilities.OrderMapsForRows(allMaps, model.UserMaps);
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

            bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true && x.UserId == userId && x.CampaignID == model.SelectedCampaign.CampaignID).Any();
            if(selectedNoteCheck == true)
            {
                model.SelectedNote = await _context.SessionNotes.Where(x => x.SelectedSessionNote == true && x.UserId == userId && x.CampaignID == model.SelectedCampaign.CampaignID).FirstAsync();
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
