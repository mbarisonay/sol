// user_panel/Controllers/QrController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// Using statements updated to match your project structure
using user_panel.Context;
using user_panel.Data; // <-- Corrected to use the 'Data' namespace for your Cabin model

namespace user_panel.Controllers
{
    public class QrController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QrController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateQrCodes()
        {
            // This now uses _context.Cabins, which is correct for your DbContext
            var cabinsToUpdate = await _context.Cabins.Where(c => string.IsNullOrEmpty(c.qr_code)).ToListAsync();
            int count = 0;

            if (cabinsToUpdate.Any())
            {
                foreach (var cabin in cabinsToUpdate)
                {
                    cabin.qr_code = Guid.NewGuid().ToString();
                    count++;
                }
                await _context.SaveChangesAsync();
                return Content($"Successfully generated and saved {count} new QR codes.");
            }
            return Content("No cabins needed a QR code to be generated.");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyQrCode([FromBody] QrRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.QrCode))
            {
                return BadRequest("QR Code is required.");
            }

            var cabin = await _context.Cabins.FirstOrDefaultAsync(c => c.qr_code == request.QrCode);

            if (cabin == null)
            {
                return NotFound(new { message = "Invalid QR Code. Cabin not found." });
            }

            // ===================================================================
            // THE FINAL FIX: Changed 'cabin.Name' to 'cabin.Description'
            // This now correctly uses the 'Description' property from YOUR Cabin.cs file.
            // ===================================================================
            return Ok(new { location = cabin.Description });
        }
    }

    // This helper class is correct and can stay here.
    public class QrRequestModel
    {
        public string? QrCode { get; set; }
    }
}