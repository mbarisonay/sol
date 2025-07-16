// user_panel/Controllers/QrController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using user_panel.Context;
using user_panel.Data; // Kabin ve Booking modelleriniz için
using Microsoft.AspNetCore.Identity; // UserManager için eklendi

namespace user_panel.Controllers
{
    public class QrController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // <-- YENİ: Kullanıcıyı yönetmek için eklendi

        // Constructor'ı UserManager'ı alacak şekilde güncelledik
        public QrController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; // <-- YENİ
        }

        // Bu metot QR kodlarını oluşturmak için, olduğu gibi kalabilir.
        [HttpGet]
        public async Task<IActionResult> GenerateQrCodes()
        {
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


        // ===================================================================
        // YENİ VE GELİŞTİRİLMİŞ METOT
        // Bu metot QR kodunu okutulduğunda rezervasyon kontrolü yapar.
        // ===================================================================
        [HttpPost]
        public async Task<IActionResult> CheckInWithQr([FromBody] QrRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.QrCode))
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var cabin = await _context.Cabins.FirstOrDefaultAsync(c => c.qr_code == request.QrCode);
            if (cabin == null)
            {
                return NotFound(new { status = "invalid_qr", message = "Invalid QR Code. Cabin not found." });
            }

            var now = DateTime.UtcNow;

            // ===================================================================
            // NİHAİ DÜZELTME BURADA YAPILDI
            // Modelinize uygun olarak 'b.ApplicationUserId' kullanıldı.
            // =================================m==================================
            var activeBooking = await _context.Bookings
                .FirstOrDefaultAsync(b =>
                    b.ApplicationUserId == currentUser.Id &&  // Bu kullanıcı için mi?
                    b.CabinId == cabin.Id &&                  // Bu kabin için mi?
                    b.StartTime <= now &&                     // Rezervasyon başladı mı?
                    b.EndTime > now);                         // Rezervasyon henüz bitmedi mi?

            if (activeBooking != null)
            {
                return Ok(new { status = "success", location = cabin.Description });
            }
            else
            {
                return Ok(new { status = "no_reservation", message = "You have no reservation." });
            }
        }
    }

    // Bu yardımcı model olduğu gibi kalabilir
    public class QrRequestModel
    {
        public string? QrCode { get; set; }
    }
}