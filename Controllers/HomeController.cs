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

        public async Task<IActionResult> WikiContent([FromQuery] int wikiPageID)
        {
            WikiPage wikiPage = await _context.WikiPages.FindAsync(wikiPageID);

            return View(wikiPage);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
