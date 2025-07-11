using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.ApplicationUserServices;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace user_panel.Controllers
{
    [Authorize]
    public class AccessController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IApplicationUserService _userService;

        public AccessController(IBookingService bookingService, IApplicationUserService userService)
        {
            _bookingService = bookingService;
            _userService = userService;
        }

        public class UnlockRequest
        {
            public int CabinId { get; set; }
        }

        [HttpPost]
        [Route("api/access/unlock")]
        public async Task<IActionResult> Unlock([FromBody] UnlockRequest request)
        {
            var currentUser = await _userService.GetCurrentUserAsync(User);
            var currentTime = DateTime.UtcNow;

            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "User not found." });
            }

            var gracePeriod = TimeSpan.FromMinutes(5);

            var validBooking = (await _bookingService.GetWhereAsync(b =>
                b.ApplicationUserId == currentUser.Id &&
                b.CabinId == request.CabinId &&
                currentTime >= (b.StartTime - gracePeriod) &&
                currentTime < b.EndTime
            )).FirstOrDefault();

            if (validBooking != null)
            {
                // ===================================================================
                // === ADD THIS LOGIC TO UPDATE THE BOOKING ===
                // 1. If this is the first successful scan, set the CheckInTime.
                // 2. Save the changes back to the database via the service.
                // ===================================================================
                if (validBooking.CheckInTime == null)
                {
                    validBooking.CheckInTime = currentTime;
                    await _bookingService.UpdateAsync(validBooking);
                }

                // TODO: Communicate with the physical smart lock's API.
                return Ok(new { success = true, message = "Access Granted. Welcome!" });
            }
            else
            {
                return Unauthorized(new { success = false, message = "Access Denied. No active booking found for this time." });
            }
        }
    }
}