using System;
using System.Collections.Generic;
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
            _targetFilePath = config.GetValue<string>("StoredFilesPath");
            _fileProvider = fileProvider;
        }

        // POST: api/Streaming
        [HttpPost("{folderName}")]
        public async Task<IActionResult> PostNewFile(string folderName)
        {
            IFormFile postedFile = Request.Form.Files[0];
            string filePath = _targetFilePath;

            if (folderName != "Root")
            {
                filePath = _targetFilePath + "\\" + folderName;
            }

            using (FileStream targetStream = System.IO.File.Create(
                            Path.Combine(filePath, postedFile.FileName)))
            {
                await postedFile.CopyToAsync(targetStream);

            }

            Map map = new Map
            {
                FileName = postedFile.FileName,
                FilePath = filePath,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _context.AddAsync(map);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
