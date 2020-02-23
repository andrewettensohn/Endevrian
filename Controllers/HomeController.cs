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


namespace Endevrian.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AdventureLog()
        {

            List<AdventureLog> adventureLogList = await _context.AdventureLogs.ToListAsync();
            adventureLogList = adventureLogList.OrderByDescending(x => x.AdventureLogID).ToList();
            List<AdventureLogViewModel> model = new List<AdventureLogViewModel>();
            foreach(AdventureLog log in adventureLogList)
            {
                AdventureLogViewModel logForDisplay = new AdventureLogViewModel
                {
                    Log = log,
                    DisplayCreateDate = log.LogDate.ToString("M/d/yyyy")
                };
                model.Add(logForDisplay);

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
