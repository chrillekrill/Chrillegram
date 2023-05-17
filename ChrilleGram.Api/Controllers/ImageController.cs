using ChrilleGram.Api.Data;
using ChrilleGram.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace ChrilleGram.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class ImageController : Controller
    {
        private readonly Context _context;
        private readonly IUserService _userService;
        public ImageController(Context context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet("[controller]/[action]")]
        public async Task<IActionResult> GetAll()
        {
            var model = await _context.Image.ToListAsync();

            return Ok(model);
        }
        [HttpGet("[controller]/[action]")]
        public IActionResult GetImage(string imagePath)
        {
            var uploadsFolder = Directory.GetCurrentDirectory();

            var filePath = Path.Combine(uploadsFolder, imagePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var mimeType = GetMimeType(filePath);

            return File(fileStream, mimeType);
        }

        private static readonly Dictionary<string, string> ImageMimeTypes = new Dictionary<string, string>
        {
            { ".bmp", "image/bmp" },
            { ".gif", "image/gif" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".svg", "image/svg+xml" },
            { ".tiff", "image/tiff" },
            { ".webp", "image/webp" }
        };
        private string GetMimeType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var mimeType))
            {
                var extension = Path.GetExtension(filePath);
                if (ImageMimeTypes.TryGetValue(extension, out var imageMimeType))
                {
                    mimeType = imageMimeType;
                }
                else
                {
                    mimeType = "application/octet-stream";
                }
            }
            return mimeType;
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery(Name = "jwt")] string jwt)
        {
            if (file != null && file.Length > 0)
            {
                var uploaderId = await _userService.GetUserIdFromToken(jwt);

                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(fileName);

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var relativeFolderPath = Path.Combine(uploadsFolder, uploaderId.ToString());
                var filePath = Path.Combine(relativeFolderPath, $"{fileNameWithoutExtension}-{uniqueFileName}");

                var uploaderFolderPath = Path.Combine(uploadsFolder, relativeFolderPath);
                if (!Directory.Exists(uploaderFolderPath))
                {
                    Directory.CreateDirectory(uploaderFolderPath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                if (System.IO.File.Exists(filePath))
                {
                    var fileToSave = $"wwwroot/uploads/{uploaderId}/{fileNameWithoutExtension}-{uniqueFileName}";
                    var img = new Entities.Image();

                    img.Path = fileToSave;

                    await _context.Image.AddAsync(img);

                    await _context.SaveChangesAsync();

                    return Ok("File uploaded successfully!");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save file.");
                }
            }

            return BadRequest("Please provide a file to upload!");
        }
    }
}
