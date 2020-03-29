using System;
using System.Collections.Generic;
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
        public async Task<IActionResult> PostNewFile()
        {
            IFormFile postedFile = Request.Form.Files[0];
            string userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (folderName != "Root")
            //{
            //    filePath = _targetFilePath + "\\" + folderName;
            //}
            string filePath = _targetFilePath + "\\" + userName;

            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            using (FileStream targetStream = System.IO.File.Create(
                            Path.Combine(filePath, postedFile.FileName)))
            {
                await postedFile.CopyToAsync(targetStream);

            }

            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Map map = new Map
            {
                FileName = postedFile.FileName,
                FilePath = $"Maps\\{currentUser}\\{postedFile.FileName}",
                UserId = currentUser
            };

            await _context.AddAsync(map);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
