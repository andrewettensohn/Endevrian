using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models.MapModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Endevrian.Controllers;
using Endevrian.Models;
using Microsoft.EntityFrameworkCore;

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/User/api/Map")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private readonly string _targetFilePath;
        private readonly IFileProvider _fileProvider;
        private readonly ApplicationDbContext _context;
        private readonly QueryHelper _queryHelper;
        private SystemLogController _logger;

        public MapController(ApplicationDbContext context, IConfiguration config, IFileProvider fileProvider, SystemLogController logger)
        {
            // To save physical files to a path provided by configuration:
            _context = context;
            //_targetFilePath = config.GetValue<string>("StoredFilesPath");
            _targetFilePath = config.GetValue<string>(WebHostDefaults.ContentRootKey) + "\\wwwroot\\UserContent\\Maps";
            _fileProvider = fileProvider;
            _logger = logger;
            _queryHelper = new QueryHelper(config, logger, context);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Map>> GetMap(int id)
        {
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Map map = await _context.Maps.FindAsync(id);
            if (map.UserId != currentUser)
            {
                return NotFound();
            }

            return map;
        }

        [HttpPut("{id}/{newMapName}")]
        public async Task<IActionResult> UpdateMapName(int id, string newMapName)
        {
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Map mapToUpdate = await _context.Maps.FindAsync(id);

            if(mapToUpdate.UserId != currentUser)
            {
                return BadRequest();
            }

            if(newMapName == "")
            {
                mapToUpdate.MapName = "Map Name";
            }
            else
            {
                mapToUpdate.MapName = newMapName;
            }

            _context.Entry(mapToUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMap(int id)
        {
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Map mapToDelete = await _context.Maps.FindAsync(id);
            
            if(mapToDelete.UserId != currentUser)
            {
                return BadRequest();
            }

            try
            {
                string targetfilePath = $"{_targetFilePath}\\{currentUser}\\{mapToDelete.FileName}";
                System.IO.File.Delete(targetfilePath);
                string targetPreviewFilePath = $"{_targetFilePath}\\{currentUser}\\{mapToDelete.PreviewFileName}";
                System.IO.File.Delete(targetPreviewFilePath);
            }
            catch(Exception exc)
            {
                _logger.AddSystemLog($"An Exception occured while deleting a map file: {exc}");
                return StatusCode(500);
            }

            _context.Remove(mapToDelete);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("LinkMap/{id}")]
        public async Task<IActionResult> LinkExistingMapToNote(int id)
        {
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Map mapToLink = await _context.Maps.FindAsync(id);

            if(currentUser != mapToLink.UserId)
            {
                return BadRequest();
            }

            Campaign activeCampaign = _queryHelper.ActiveCampaignQuery(currentUser);
            SessionNote relatedSessionNote = _context.SessionNotes.Where(x => x.SelectedSessionNote == true).First();

            mapToLink.SessionNoteID = relatedSessionNote.SessionNoteID;

            _context.Entry(mapToLink).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PostNewMap()
        {
            IFormFile postedFile = Request.Form.Files[0];
            bool foundMapName = Request.Form.TryGetValue("mapName", out StringValues mapNameValues);

            if (!foundMapName)
            {
                return BadRequest();
            }

            string mapName = mapNameValues.AsEnumerable().First();
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string targetfilePath = $"{_targetFilePath}\\{currentUser}";

            if(!Directory.Exists(targetfilePath))
            {
                Directory.CreateDirectory(targetfilePath);
            }

            using (FileStream targetStream = System.IO.File.Create(
                            Path.Combine(targetfilePath, postedFile.FileName)))
            {
                await postedFile.CopyToAsync(targetStream);

            }

            string filePath = $"{_targetFilePath}\\{currentUser}\\{postedFile.FileName}";

            Bitmap bmp = new Bitmap(filePath);
            VaryQualityLevel(bmp, currentUser, postedFile.FileName);

            Map map = new Map
            {
                CampaignID = _queryHelper.ActiveCampaignQuery(currentUser).CampaignID,
                FileName = postedFile.FileName,
                FilePath = $"Maps\\{currentUser}\\{postedFile.FileName}",
                UserId = currentUser,
                MapName = mapName,
                PreviewFilePath = $"Maps\\{ currentUser}\\Preview{ postedFile.FileName}",
                PreviewFileName = $"Preview{postedFile.FileName}"
            };

            bool foundNote = Request.Form.TryGetValue("noteId", out StringValues noteValues);
            if (foundNote)
            {
                map = await LinkNewMapToNote(noteValues, map);
            }

            await _context.AddAsync(map);
            await _context.SaveChangesAsync();

            return Ok();
            //return CreatedAtAction("GetMap", new { id = map.MapID }, map);
        }

        private async Task<Map> LinkNewMapToNote(StringValues noteValues, Map map)
        {
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string noteIdString = noteValues.AsEnumerable().First();
            bool parseNoteId = int.TryParse(noteIdString, out int noteId);

            if (parseNoteId)
            {
                SessionNote noteToLink = await _context.SessionNotes.FindAsync(noteId);
                if (noteToLink.UserId == currentUser)
                {
                    map.SessionNoteID = noteToLink.SessionNoteID;
                }
            }

            return map;
        }

        private void VaryQualityLevel(Bitmap bmp, string currentUser, string fileName)
        {
            using (bmp)
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
  
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 25L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp.Save(@$"{_targetFilePath}\\{currentUser}\\Preview{fileName}", jpgEncoder, myEncoderParameters);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
