﻿using ChrilleGram.Api.Data;
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
        public async Task<IActionResult> UploadFile(IFormFile file, string jwt)
        {
            // Check if the file is not null and has content
            if (file != null && file.Length > 0)
            {
                var uploaderId = await _userService.GetUserIdFromToken(jwt);

                // Get the file name and extension
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(fileName);

                // Generate a unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                // Get the path of the folder where you want to save the file
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // Create the directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Get the path of the file where you want to save the uploaded file
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var relativeFolderPath = Path.Combine(uploadsFolder, uploaderId.ToString());
                var filePath = Path.Combine(relativeFolderPath, $"{fileNameWithoutExtension}-{uniqueFileName}");

                var uploaderFolderPath = Path.Combine(uploadsFolder, relativeFolderPath);
                if (!Directory.Exists(uploaderFolderPath))
                {
                    Directory.CreateDirectory(uploaderFolderPath);
                }

                // Save the file to the file path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Check if the file was created
                if (System.IO.File.Exists(filePath))
                {
                    // Return a success response
                    var fileToSave = $"wwwroot/uploads/{uploaderId}/{fileNameWithoutExtension}-{uniqueFileName}";
                    var img = new Entities.Image();

                    img.Path = fileToSave;

                    await _context.Image.AddAsync(img);

                    await _context.SaveChangesAsync();

                    return Ok("File uploaded successfully!");
                }
                else
                {
                    // Return a server error response if the file was not created
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save file.");
                }
            }

            // Return a bad request response if the file is null or has no content
            return BadRequest("Please provide a file to upload!");
        }
    }
}
