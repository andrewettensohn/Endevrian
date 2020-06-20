using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Endevrian.Models;
using Microsoft.EntityFrameworkCore;
using Endevrian.Data;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Endevrian.Models.WikiModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                //campaign.WikiPages
            }

            if(model.Campaigns == null)
            {
                model.Campaigns = new List<Campaign>();
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
