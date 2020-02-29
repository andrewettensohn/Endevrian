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
        //private readonly ILogger<HomeController> _logger;
        private readonly SystemLogController _logger;
        private readonly ApplicationDbContext _context;
        private readonly QueryHelper _queryHelper;

        public HomeController(ApplicationDbContext context, IConfiguration configuration, SystemLogController logger)
        {
            _logger = logger;
            _context = context;
            _queryHelper = new QueryHelper(configuration, logger);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdventureLog()
        {

            AdventureLogViewModel model = new AdventureLogViewModel();

            Campaign selectedCampaign = _queryHelper.ActiveCampaignQuery();
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

            Campaign SelectedCampaign = _queryHelper.ActiveCampaignQuery();

            if(SelectedCampaign.IsSelectedCampaign == true && SelectedCampaign.UserId == userId)
            {
                model.SelectedCampaign = SelectedCampaign;
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
