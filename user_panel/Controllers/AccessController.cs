using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.ApplicationUserServices;
using System;
using System.Threading.Tasks;
using System.Linq;
using user_panel.Services.Entity.CabinServices; // --- 1. ADD THIS using statement ---

namespace user_panel.Controllers
{
    [Authorize]
    public class AccessController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IApplicationUserService _userService;
        private readonly ICabinService _cabinService; // --- 2. ADD THIS SERVICE FIELD ---

        // --- 3. UPDATE THE CONSTRUCTOR TO INJECT THE CABIN SERVICE ---
        public AccessController(IBookingService bookingService, IApplicationUserService userService, ICabinService cabinService)
        {
            _bookingService = bookingService;
            _userService = userService;
            _cabinService = cabinService; // --- And assign it here
        }

        // --- 4. UPDATE THE REQUEST MODEL TO ACCEPT THE QR CODE STRING ---
        public class UnlockRequest
        {
            public string QrCodeData { get; set; }
        }

        [HttpPost]
        [Route("api/access/unlock")]
        // --- 5. REPLACE THE UNLOCK METHOD WITH THIS NEW LOGIC ---
        public async Task<IActionResult> Unlock([FromBody] UnlockRequest request)
        {
            // First, get the currently logged-in user
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated. Please log in." });
            }

            if (string.IsNullOrWhiteSpace(request.QrCodeData))
            {
                return BadRequest(new { success = false, message = "QR Code data was empty." });
            }

            // Next, find the cabin using the QR Code string from the request
            var cabin = await _cabinService.GetCabinByQrCodeAsync(request.QrCodeData);
            if (cabin == null)
            {
                // This means the QR code is invalid or doesn't belong to any cabin
                return NotFound(new { success = false, message = "Invalid QR Code. This cabin could not be found." });
            }

            // Now, with the correct cabin found, use its ID to check for a valid booking
            var currentTime = DateTime.UtcNow;
            var gracePeriod = TimeSpan.FromMinutes(15); // A reasonable check-in window

            var validBooking = (await _bookingService.GetWhereAsync(b =>
                b.ApplicationUserId == currentUser.Id &&
                b.CabinId == cabin.Id && // Use the ID from the cabin we just found
                currentTime >= (b.StartTime - gracePeriod) &&
                currentTime < b.EndTime
            )).FirstOrDefault();

            if (validBooking != null)
            {
                // SUCCESS! The user has a valid reservation.
                return Ok(new { success = true, message = $"Welcome to the sport cabin. Enjoy your workout!" });
            }
            else
            {
                // FAILURE! No valid booking was found for this user/cabin/time.
                return Unauthorized(new { success = false, message = "You don't have a reservation for this cabin at this time." });
            }
        }
    }
}