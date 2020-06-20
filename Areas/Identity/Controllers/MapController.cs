using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Endevrian.Data;
using Endevrian.Models.MapModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Endevrian.Controllers;
using Endevrian.Models;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/Author/api/Map")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private readonly IFileProvider _fileProvider;
        private readonly ApplicationDbContext _context;
        private SystemLogController _logger;
        BlobServiceClient blobServiceClient;

        public MapController(ApplicationDbContext context, IConfiguration config, IFileProvider fileProvider, SystemLogController logger)
        {
            _context = context;
            _fileProvider = fileProvider;
            _logger = logger;
            blobServiceClient = new BlobServiceClient(config.GetConnectionString("FileConnection"));
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
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(currentUser);
                await containerClient.DeleteBlobAsync(mapToDelete.FileName);
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

            Campaign selectedCampaign = _context.Campaigns.First(x => x.UserId == currentUser);
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

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(currentUser);

            if(!containerClient.Exists())
            {
                containerClient = await blobServiceClient.CreateBlobContainerAsync(currentUser);
                containerClient.SetAccessPolicy(PublicAccessType.BlobContainer);
            }

            BlobClient primaryBlobClient = containerClient.GetBlobClient(postedFile.FileName);
            

            // Open the file and upload its data
            using Stream uploadFileStream = postedFile.OpenReadStream();
            await primaryBlobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            Map map = new Map
            {
                CampaignID = _context.Campaigns.First(x => x.UserId == currentUser).CampaignID,
                FileName = postedFile.FileName,
                FilePath = primaryBlobClient.Uri.ToString(),
                UserId = currentUser,
                MapName = mapName,
            };

            bool foundNote = Request.Form.TryGetValue("noteId", out StringValues noteValues);
            if (foundNote)
            {
                map = await LinkNewMapToNote(noteValues, map);
            }

            await _context.AddAsync(map);
            await _context.SaveChangesAsync();

            return Ok();
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

        //private string UploadMapPreview()
        //{



        //}

        //private async Task<string> VaryQualityLevel(Bitmap bmp, string fileName, BlobContainerClient containerClient)
        //{
        //    using (bmp)
        //    {
        //        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
  
        //        Encoder myEncoder = Encoder.Quality;
        //        EncoderParameters myEncoderParameters = new EncoderParameters(1);

        //        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 25L);
        //        myEncoderParameters.Param[0] = myEncoderParameter;

        //        BlobClient blobClient = containerClient.GetBlobClient($"Preview{fileName}");
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            bmp.Save(memoryStream, jpgEncoder, myEncoderParameters);
        //            memoryStream.Seek(0, SeekOrigin.Begin);
        //            await blobClient.UploadAsync(memoryStream, true);
        //        }

        //        return blobClient.Uri.ToString();
        //    }
        //}

        //private ImageCodecInfo GetEncoder(ImageFormat format)
        //{
        //    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        //    foreach (ImageCodecInfo codec in codecs)
        //    {
        //        if (codec.FormatID == format.Guid)
        //        {
        //            return codec;
        //        }
        //    }
        //    return null;
        //}
    }
}
