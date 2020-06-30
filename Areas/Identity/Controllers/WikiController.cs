using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Endevrian.Data;
using Endevrian.Models.WikiModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Endevrian.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Identity/Author/api/WikiPage")]
    [ApiController]
    public class WikiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        BlobServiceClient blobServiceClient;

        public WikiController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            blobServiceClient = new BlobServiceClient(config.GetConnectionString("FileConnection"));
        }

        // POST: api/WikiPage
        [HttpPost]
        public async Task<IActionResult> PostWikiPage()
        {
            
            bool parseCampaignId = int.TryParse(Request.Form["campaignID"], out int sentCampaignId);
            bool parseWikiPageId = int.TryParse(Request.Form["wikiPageID"], out int sentWikiPageId);
            if (!parseCampaignId || !parseWikiPageId || sentCampaignId == 0) 
            {
                return BadRequest();
            }

            WikiPage sentWikiPage = new WikiPage
            {
                CampaignID = sentCampaignId,
                WikiPageID = sentWikiPageId,
                WikiContent = (Request.Form["wikiContent"] == "") ? "No Content has been added to this page yet." : Request.Form["wikiContent"].ToString(),
                PageName = (Request.Form["pageName"] == "") ? "New Page" : Request.Form["pageName"].ToString(),
                ImageFile = (Request.Form.Files.Count > 0) ? Request.Form.Files[0] : null
            };

            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (sentWikiPage.WikiPageID != 0)
            {
                //Update Existing Wiki Page
                WikiPage currentWikiPage = await _context.WikiPages.FindAsync(sentWikiPage.WikiPageID);
                
                if(currentWikiPage.UserId != currentUser)
                {
                    return BadRequest();
                }

                if(currentWikiPage.ImagePath != null && sentWikiPage.ImageFile != null)
                {
                    await DeleteOldImageBlobIfNotEqual(sentWikiPage);
                    sentWikiPage.ImagePath = await UploadWikiImage(sentWikiPage);
                }

                _context.Entry(sentWikiPage).State = EntityState.Modified;
            }
            else
            {
                //Create New Wiki Page
                if(!_context.Campaigns.Where(x => x.CampaignID == sentWikiPage.CampaignID).Any())
                {
                    return BadRequest();
                }

                sentWikiPage.UserId = currentUser;

                if (sentWikiPage.ImageFile != null)
                {
                    sentWikiPage.ImagePath = await UploadWikiImage(sentWikiPage);
                }

                await _context.AddAsync(sentWikiPage);
            }

            await _context.SaveChangesAsync();

            return Ok(sentWikiPage);
        }

        private async Task<IActionResult> DeleteOldImageBlobIfNotEqual(WikiPage wikiPage)
        {

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(wikiPage.UserId);
            BlobClient blobClient = containerClient.GetBlobClient(wikiPage.PageName);
            var blobProps = await blobClient.GetPropertiesAsync();

            int oldSize = Convert.ToInt32(blobProps.Value.ContentLength);

            if(oldSize != wikiPage.ImageFile.Length)
            {
                await containerClient.DeleteBlobAsync(wikiPage.PageName);
            }

            return Ok();
        }

        private async Task<string> UploadWikiImage(WikiPage wikiPage)
        {
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(wikiPage.UserId);

            if (!containerClient.Exists())
            {
                containerClient = await blobServiceClient.CreateBlobContainerAsync(wikiPage.UserId);
                containerClient.SetAccessPolicy(PublicAccessType.BlobContainer);
            }

            BlobClient primaryBlobClient = containerClient.GetBlobClient(wikiPage.PageName);

            // Open the file and upload its data
            using Stream uploadFileStream = wikiPage.ImageFile.OpenReadStream();
            await primaryBlobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            return primaryBlobClient.Uri.ToString();
        }
    }
}
