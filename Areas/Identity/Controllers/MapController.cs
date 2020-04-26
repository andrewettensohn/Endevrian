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

        public MapController(ApplicationDbContext context, IConfiguration config, IFileProvider fileProvider, SystemLogController logger)
        {
            // To save physical files to a path provided by configuration:
            _context = context;
            //_targetFilePath = config.GetValue<string>("StoredFilesPath");
            _targetFilePath = config.GetValue<string>(WebHostDefaults.ContentRootKey) + "\\wwwroot\\UserContent\\Maps";
            _fileProvider = fileProvider;
            _queryHelper = new QueryHelper(config, logger);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Map>> GetMap(int id)
        {

            Map map = await _context.Maps.FindAsync(id);
            return map;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMap()
        {

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
                string noteIdString = noteValues.AsEnumerable().First();
                bool parseNoteId = int.TryParse(noteIdString, out int noteId);

                if(parseNoteId)
                {
                    SessionNote noteToLink = await _context.SessionNotes.FindAsync(noteId);
                    if(noteToLink.UserId == currentUser)
                    {
                        map.RelatedSessionNotes.Add(noteToLink);
                    }
                }
            }

            await _context.AddAsync(map);
            await _context.SaveChangesAsync();

            return Ok();
            //return CreatedAtAction("GetMap", new { id = map.MapID }, map);
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
