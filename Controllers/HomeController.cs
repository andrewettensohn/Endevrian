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

        //public IActionResult AdventureLog()
        //{

        //    AdventureLogViewModel model = new AdventureLogViewModel();

        //    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    model.SelectedCampaign = _context.Campaigns.FirstOrDefault(x => x.UserId == userId && x.IsSelectedCampaign == true);

        //    if (model.SelectedCampaign != null)
        //    {
        //        List<AdventureLog> adventureLogList = _context.AdventureLogs.Where(x => x.CampaignID == model.SelectedCampaign.CampaignID).ToList();
        //        adventureLogList = adventureLogList.OrderByDescending(x => x.AdventureLogID).ToList();

        //        model.AdventureLogs = adventureLogList;

        //    }
        //    else
        //    {
        //        model.SelectedCampaign = new Campaign { IsSelectedCampaign = false };
        //    }

        //    return View(model);

        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
