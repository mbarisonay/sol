using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using user_panel.Data;
using System;
using System.Linq; // Added for .ToList() if needed
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List<T>
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;

namespace user_panel.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ICabinService _cabinService;
        private readonly IApplicationUserService _userService;

        public BookingController(IBookingService bookingService, ICabinService cabinService, IApplicationUserService userService)
        {
            _bookingService = bookingService;
            _cabinService = cabinService;
            _userService = userService;
        }

        // --- SIMPLIFIED INDEX ACTION ---
        // The controller's only responsibility is to get ALL cabins and send them to the view.
        // The view's JavaScript will handle the live filtering.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Using ToList() is a safe practice in case the base method returns IEnumerable.
            var cabins = (await _cabinService.GetAllAsync()).ToList();
            return View(cabins);
        }

        // --- UNCHANGED ACTIONS BELOW ---

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var cabin = await _cabinService.GetByIdAsync(id);
            if (cabin == null) return NotFound();
            return View(cabin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cabinId, DateTime bookingDate, int startTimeHour)
        {
            var cabin = await _cabinService.GetByIdAsync(cabinId);
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (cabin == null || currentUser == null) return NotFound();

            var bookingStartTime = bookingDate.Date.AddHours(startTimeHour);
            var bookingEndTime = bookingStartTime.AddHours(1);
            var bookingCost = cabin.PricePerHour;

            if (currentUser.CreditBalance < bookingCost)
            {
                TempData["ErrorMessage"] = $"Insufficient funds. Your balance is {currentUser.CreditBalance:C}, but the booking costs {bookingCost:C}.";
                return RedirectToAction("Create", new { id = cabinId });
            }

            var overlapping = await _bookingService
                .AnyAsync(b => b.CabinId == cabinId &&
                               bookingStartTime < b.EndTime &&
                               bookingEndTime > b.StartTime);

            if (overlapping)
            {
                TempData["ErrorMessage"] = "This time slot is already booked. Please choose another time.";
                return RedirectToAction("Create", new { id = cabinId });
            }

            var newBooking = new Booking
            {
                ApplicationUserId = currentUser.Id,
                CabinId = cabinId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime
            };

            currentUser.CreditBalance -= bookingCost;
            await _bookingService.CreateAsync(newBooking);
            await _userService.UpdateAsync(currentUser);

            TempData["SuccessMessage"] = $"Success! Your booking for {cabin.Location} on {bookingStartTime:f} is confirmed.";
            return RedirectToAction("MyBookings");
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            var bookings = await _bookingService.GetAllWithCabinForUserAsync(user.Id);
            return View(bookings);
        }
    }
}