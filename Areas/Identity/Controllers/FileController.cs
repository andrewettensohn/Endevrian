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

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/User/api/File")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string _targetFilePath;
        private readonly IFileProvider _fileProvider;
        private readonly ApplicationDbContext _context;

        public FileController(ApplicationDbContext context, IConfiguration config, IFileProvider fileProvider)
        {
            // To save physical files to a path provided by configuration:
            _context = context;
            //_targetFilePath = config.GetValue<string>("StoredFilesPath");
            _targetFilePath = config.GetValue<string>(WebHostDefaults.ContentRootKey) + "\\wwwroot\\UserContent\\Maps";
            _fileProvider = fileProvider;
        }

        //[HttpGet]
        //public IActionResult GetMapFiles()
        //{
        //    string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var fileList = _fileProvider.GetDirectoryContents("\\Maps\\" + userName);

        //    return PhysicalFile(f)
        //}

        // POST: api/Streaming
        [HttpPost]
        public async Task<IActionResult> PostNewMap()
        {
            IFormFile postedFile = Request.Form.Files[0];
            bool foundMap = Request.Form.TryGetValue("mapName", out StringValues mapValues);

            if(!foundMap)
            {
                return BadRequest();
            }

            string mapName = mapValues.AsEnumerable().First();
            //bool foundMapName = Request.Form.TryGetValue("MapName", out StringValues mapNameFromRequest);

            //if(!foundMapName)
            //{
            //    return BadRequest();
            //}

            //string mapName = mapNameFromRequest.ToString();


            string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

            string targetfilePath = _targetFilePath + "\\" + userName;

            if(!Directory.Exists(targetfilePath))
            {
                Directory.CreateDirectory(targetfilePath);
            }

            using (FileStream targetStream = System.IO.File.Create(
                            Path.Combine(targetfilePath, postedFile.FileName)))
            {
                await postedFile.CopyToAsync(targetStream);

            }

            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            string filePath = $"{_targetFilePath}\\{currentUser}\\{postedFile.FileName}";

            Bitmap bmp = new Bitmap(filePath);
            VaryQualityLevel(bmp, currentUser, postedFile.FileName);

            Map map = new Map
            {
                FileName = postedFile.FileName,
                FilePath = $"Maps\\{currentUser}\\{postedFile.FileName}",
                UserId = currentUser,
                MapName = mapName,
                PreviewFilePath = $"Maps\\{ currentUser}\\Preview{ postedFile.FileName}",
                PreviewFileName = $"Preview{postedFile.FileName}"
            };

            await _context.AddAsync(map);
            await _context.SaveChangesAsync();

            return Ok();
            //return CreatedAtAction("GetMap", new { id = map.MapID }, map);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Map>> GetMap(int id)
        {

            Map map = await _context.Maps.FindAsync(id);
            return map;
        }

        private void VaryQualityLevel(Bitmap bmp, string currentUser, string fileName)
        {
            using (bmp)
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
  
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 0L);
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
