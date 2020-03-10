using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models;
using Microsoft.AspNetCore.Mvc;


namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/User/api/SessionSection")]
    [ApiController]
    public class SessionSectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        SessionSectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> PostSessionSection([FromBody]SessionSection sessionSection)
        {
            sessionSection.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Campaign selectedCampaign = new Campaign();

            try
            {
                selectedCampaign = await _context.Campaigns.FindAsync(sessionSection.CampaignID);
            }
            catch
            {

            }

            //if(sessionSection.CampaignID)
            

        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
