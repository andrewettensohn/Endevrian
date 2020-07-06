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
using Endevrian.Models.TagModels;
using Endevrian.Models.WikiModels;
using Endevrian.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class AuthorController : Controller
    {
        private readonly SystemLogController _logger;
        private readonly ApplicationDbContext _context;

        public AuthorController(ApplicationDbContext context, SystemLogController logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> NewAdventureLog([FromQuery] int adventureLogID)
        {
            AdventureLog log = await _context.AdventureLogs.FindAsync(adventureLogID);
            return View(log);
        }

        public async Task<IActionResult> CampaignList()
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            CampaignViewModel model = new CampaignViewModel
            {
                Campaigns = await _context.Campaigns.Where(x => x.UserId == userId).ToListAsync()
            };

            if(model.Campaigns.Count != 0)
            {
                model.SelectedCampaign = _context.Campaigns.FirstOrDefault(x => x.UserId == userId && x.IsSelectedCampaign == true);
            }

            return View(model);
        }

        public async Task<IActionResult> NewMap()
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign SelectedCampaign = _context.Campaigns.First(x => x.UserId == userId);

            NewMapViewModel model = new NewMapViewModel
            {
                SelectedCampaignID = SelectedCampaign.CampaignID,
                SelectedCampaignSessionSections = await _context.SessionSections.Where(x => x.CampaignID == SelectedCampaign.CampaignID && userId == x.UserId).ToListAsync(),
                SelectedCampaignSessionNotes = await _context.SessionNotes.Where(x => x.CampaignID == SelectedCampaign.CampaignID && userId == x.UserId).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> LinkMap(string searchString)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MapViewModel model = new MapViewModel
            {
                UserMaps = new List<List<Map>>()
            };

            if (searchString != null)
            {
                List<Map> foundMaps = Utilities.GetMapGallery(userId, searchString, _context);
                model.UserMaps = Utilities.OrderMapsForRows(foundMaps, model.UserMaps);

            }
            else
            {
                model.SelectedSessionNote = await _context.SessionNotes.Where(x => x.UserId == userId && x.SelectedSessionNote == true).FirstAsync();
                List<Map> allMaps = await _context.Maps.Where(x => x.UserId == userId).ToListAsync();
                foreach(Map map in allMaps)
                {
                    bool foundRelatedNote = await _context.SessionNotes.Where(x => x.SessionNoteID == map.SessionNoteID).AnyAsync();

                    if(foundRelatedNote)
                    {
                        map.RelatedSessionNote = await _context.SessionNotes.Where(x => x.SessionNoteID == map.SessionNoteID).FirstAsync();
                    }
                }
                model.UserMaps = Utilities.OrderMapsForRows(allMaps, model.UserMaps);
            }

            return View(model);
        }

        public async Task<IActionResult> MapGallery(string searchString)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MapViewModel model = new MapViewModel
            {
                UserMaps = new List<List<Map>>(),
                SelectedCampaign = _context.Campaigns.FirstOrDefault(x => x.UserId == userId && x.IsSelectedCampaign == true)
            };

            if(model.SelectedCampaign != null)
            {
                if (searchString != null)
                {
                    List<Map> foundMaps = Utilities.GetMapGallery(userId, searchString, _context);
                    model.UserMaps = Utilities.OrderMapsForRows(foundMaps, model.UserMaps);
                }
                else
                {

                    List<Map> allMaps = Utilities.GetMapGallery(userId, "", _context);
                    foreach (Map map in allMaps)
                    {
                        bool foundRelatedNote = await _context.SessionNotes.Where(x => x.SessionNoteID == map.SessionNoteID).AnyAsync();
                        if (foundRelatedNote)
                        {
                            map.RelatedSessionNote = await _context.SessionNotes.Where(x => x.SessionNoteID == map.SessionNoteID).FirstAsync();
                        }
                    }
                    model.UserMaps = Utilities.OrderMapsForRows(allMaps, model.UserMaps);
                }
            }
            else
            {
                model.SelectedCampaign = new Campaign { IsSelectedCampaign = false };
            }

            return View(model);
        }

        public async Task<IActionResult> SessionNotes()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SessionPlanViewModel model = new SessionPlanViewModel();
            model.SelectedCampaign = _context.Campaigns.Where(x => x.UserId == userId && x.IsSelectedCampaign == true).FirstOrDefault();

            if(model.SelectedCampaign != null)
            {
                model.SessionSections = await _context.SessionSections.Where(x => x.CampaignID == model.SelectedCampaign.CampaignID).ToListAsync();
                model.SessionNotes = await _context.SessionNotes.Where(x => x.CampaignID == model.SelectedCampaign.CampaignID).ToListAsync();

                bool selectedNoteCheck = _context.SessionNotes.Where(x => x.SelectedSessionNote == true && x.UserId == userId && x.CampaignID == model.SelectedCampaign.CampaignID).Any();
                if (selectedNoteCheck == true)
                {
                    model.SelectedNote = await _context.SessionNotes.Where(x => x.SelectedSessionNote == true && x.UserId == userId && x.CampaignID == model.SelectedCampaign.CampaignID).FirstAsync();
                    bool foundRelatedMap = await _context.Maps.Where(x => x.SessionNoteID == model.SelectedNote.SessionNoteID).AnyAsync();
                    if (foundRelatedMap)
                    {
                        model.SelectedNoteRelatedMap = await _context.Maps.Where(x => x.SessionNoteID == model.SelectedNote.SessionNoteID).FirstAsync();
                    }
                }
                else
                {
                    model.SelectedNote = null;
                }
            }
            else
            {
                model.SelectedCampaign = new Campaign { IsSelectedCampaign = false };
            }

            return View(model);
        }

        public async Task<IActionResult> Tags()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Tag> userTags = await _context.Tags.Where(x => x.UserId == userId).ToListAsync();

            return View(userTags);
        }

        public async Task<IActionResult> NewWikiPage([FromQuery] int campaignID, [FromQuery] int wikiPageID)
        {
            NewWikiPageViewModel model = new NewWikiPageViewModel
            {
                WikiPage = await _context.WikiPages.FindAsync(wikiPageID),
                Campaign = await _context.Campaigns.FindAsync(campaignID)
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
