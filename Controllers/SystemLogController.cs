using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Endevrian.Controllers
{

    public class SystemLogController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public SystemLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: SystemLog
        [HttpPost]
        public void AddSystemLog(string message)
        {
            SystemLog systemLog = new SystemLog
            {
                Message = message,
                LogTime = DateTime.Now
            };

            _context.SystemLogs.Add(systemLog);
            _context.SaveChanges();

            return;
        }
    }
}