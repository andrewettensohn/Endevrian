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

        // GET: api/WikiPage
        [HttpGet]
        public async Task<ActionResult<WikiPage>> GetWikiPage(int WikiPageID)
        {
            return await _context.WikiPages.FindAsync(WikiPageID);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWikiPage(int id)
        {
            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WikiPage pageToDelete = await _context.WikiPages.FindAsync(id);

            if(pageToDelete.UserId != currentUser)
            {
                return BadRequest();
            }

            _context.Remove(pageToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/WikiPage
        [HttpPost]
        public async Task<ActionResult<WikiPage>> PostWikiPage()
        {
            bool parseCampaignId = int.TryParse(Request.Form["campaignID"], out int sentCampaignId);
            bool parseWikiPageId = int.TryParse(Request.Form["wikiPageID"], out int sentWikiPageId);
            if (!parseCampaignId || !parseWikiPageId || sentCampaignId == 0) 
            {
                return BadRequest();
            }

            WikiPage sentWikiPage = new WikiPage
            {
                WikiContent = string.IsNullOrWhiteSpace(Request.Form["wikiContent"]) ? "No Content has been added to this page yet." : Request.Form["wikiContent"].ToString(),
                PageName = string.IsNullOrWhiteSpace(Request.Form["pageName"]) ? "New Page" : Request.Form["pageName"].ToString(),
                ImageFile = (Request.Form.Files.Count > 0) ? Request.Form.Files[0] : null,
                CardContent = Request.Form["cardContent"].ToString()
            };

            string currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (sentWikiPageId != 0)
            {
                //Update Existing Wiki Page
                WikiPage currentWikiPage = _context.WikiPages.Find(sentWikiPageId);

                currentWikiPage.WikiContent = sentWikiPage.WikiContent;
                currentWikiPage.PageName = sentWikiPage.PageName;
                currentWikiPage.CardContent = sentWikiPage.CardContent;

                if (currentWikiPage.UserId != currentUser)
                {
                    return BadRequest();
                }

                if (!string.IsNullOrWhiteSpace(currentWikiPage.ImagePath) && sentWikiPage.ImageFile != null)
                {
                    currentWikiPage.ImageFile = sentWikiPage.ImageFile;
                    await DeleteOldImageBlobIfNotEqual(currentWikiPage);
                    currentWikiPage.ImagePath = await UploadWikiImage(currentWikiPage);
                }

                _context.Entry(currentWikiPage).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    //return currentWikiPage;
                    currentWikiPage.ImageFile = null;
                    return currentWikiPage;
                    //return CreatedAtAction("GetWikiPage", new { id = sentWikiPage.WikiPageID }, sentWikiPage);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WikiPageExists(currentWikiPage.WikiPageID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                //Create New Wiki Page
                if(!_context.Campaigns.Where(x => x.CampaignID == sentCampaignId).Any())
                {
                    return BadRequest();
                }

                sentWikiPage.UserId = currentUser;
                sentWikiPage.CampaignID = sentCampaignId;
                if (sentWikiPage.ImageFile != null)
                {
                    sentWikiPage.ImagePath = await UploadWikiImage(sentWikiPage);
                }

                await _context.AddAsync(sentWikiPage);
                await _context.SaveChangesAsync();

                sentWikiPage.ImageFile = null;
                return sentWikiPage;
            }
        }

        private bool WikiPageExists(int id)
        {
            return _context.WikiPages.Any(e => e.WikiPageID == id);
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
