using ChrilleGram.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrilleGram.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : Controller
    {
        private readonly Context _context;
        public ImageController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var images = await _context.Image.ToListAsync();
            return Ok();
        }
    }
}
