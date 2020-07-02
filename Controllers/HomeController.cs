using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Endevrian.Models;
using Endevrian.Data;
using Endevrian.Models.WikiModels;

namespace Endevrian.Controllers
{
    public class HomeController : Controller
    {
        private readonly SystemLogController _logger;
        private ApplicationDbContext _context;

        public HomeController(SystemLogController logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdventureLog()
        {
            AdventureLogViewModel model = new AdventureLogViewModel
            {
                AdventureLogs = _context.AdventureLogs.ToList()
            };

            return View(model);
        }

        public IActionResult WikiPortal()
        {
            WikiPortalViewModel model = new WikiPortalViewModel
            {
                Campaigns = _context.Campaigns.ToList()
            };

            foreach (Campaign campaign in model.Campaigns)
            {
                campaign.WikiPages = _context.WikiPages.Where(x => x.CampaignID == campaign.CampaignID).OrderBy(x => x.PageName).ToList();

                if(campaign.WikiPages is null)
                {
                    campaign.WikiPages = new List<WikiPage>();
                }
            }

            if(model.Campaigns is null)
            {
                model.Campaigns = new List<Campaign>();
            }

            return View(model);
        }
        
        public async Task<IActionResult> WikiContent([FromQuery] int? wikiPageID, string searchQuery)
        {
            WikiContentViewModel model = new WikiContentViewModel();

            if(wikiPageID != null)
            {
                model.SelectedPage = await _context.WikiPages.FindAsync(wikiPageID);
            }
            else if (searchQuery != null)
            {
                searchQuery = searchQuery.ToLower();
                List<WikiPage> searchResults = _context.WikiPages.Where(
                x => x.PageName.ToLower().StartsWith(searchQuery)
                || x.PageName.ToLower().EndsWith(searchQuery)
                || x.PageName.ToLower().Contains(searchQuery)).ToList();

                if(searchResults.Count != 1)
                {
                    //TempData["searchResults"] = searchResults;
                    //return RedirectToAction("WikiSearchResults");
                    model.SelectedPage = null;
                    model.SearchResults = searchResults;
                }
                else
                {
                    model.SelectedPage = searchResults.FirstOrDefault();
                }
            }

            return View(model);
        }

        //public async Task<IActionResult> WikiSearchResults(List<WikiPage> passedSearchResults)
        //{
        //    List<WikiPage> searchResults = (List<WikiPage>)TempData["searchResults"];
        //    return View(searchResults);
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
