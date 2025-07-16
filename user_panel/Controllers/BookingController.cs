using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.ViewModels;
using System.Runtime.InteropServices;

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

        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            List<Cabin> cabins;
            if (!string.IsNullOrEmpty(searchString))
            {
                cabins = await _cabinService.SearchAsync(searchString);
            }
            else
            {
                cabins = (await _cabinService.GetCabinsWithLocationAsync()).ToList();
            }
            return View(cabins);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) { return NotFound(); }
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id.Value);
            if (cabin == null) { return NotFound(); }
            return View(cabin);
        }

        // ===================================================================
        // === THIS ACTION IS MODIFIED ===
        // ===================================================================
        [HttpGet]
        public async Task<IActionResult> Create(int id, DateTime? bookingDate)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id);
            if (cabin == null) return NotFound();

            // Logic to ensure the selected date is not in the past
            var date = (bookingDate.HasValue && bookingDate.Value.Date >= DateTime.Today)
                ? bookingDate.Value.Date
                : DateTime.Today;

            var bookingsForDate = await _bookingService.GetWhereAsync(b =>
                b.CabinId == id && b.StartTime.Date == date);

            var viewModel = new CreateBookingViewModel
            {
                Cabin = cabin,
                BookingDate = date,
                BookedHours = bookingsForDate.Select(b => b.StartTime.Hour).ToList(),

                // --- NEW LINE ADDED HERE ---
                // Set the minimum date to today, formatted for the HTML input.
                MinBookingDate = DateTime.Today.ToString("yyyy-MM-dd")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cabinId, DateTime bookingDate, int startTimeHour)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(cabinId);
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (cabin == null || currentUser == null) return NotFound();

            var bookingStartTime = bookingDate.Date.AddHours(startTimeHour);
            var bookingEndTime = bookingStartTime.AddHours(1);

            // Check against current time to prevent booking past hours on the same day
            if (bookingStartTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Cannot book a time slot in the past.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            var bookingCost = cabin.PricePerHour;
            if (currentUser.CreditBalance < bookingCost)
            {
                TempData["ErrorMessage"] = $"Insufficient funds. Your balance is {currentUser.CreditBalance:C}, but the booking costs {bookingCost:C}.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            // Convert to UTC before checking for overlaps and saving
            var bookingStartTimeUtc = bookingStartTime.ToUniversalTime();
            var bookingEndTimeUtc = bookingEndTime.ToUniversalTime();

            var overlapping = await _bookingService.AnyAsync(b =>
                b.CabinId == cabinId &&
                bookingStartTimeUtc < b.EndTime &&
                bookingEndTimeUtc > b.StartTime);
            if (overlapping)
            {
                TempData["ErrorMessage"] = "This time slot is already booked. Please choose another time.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            var newBooking = new Booking
            {
                ApplicationUserId = currentUser.Id,
                CabinId = cabinId,
                StartTime = bookingStartTimeUtc,
                EndTime = bookingEndTimeUtc
            };
            currentUser.CreditBalance -= bookingCost;
            await _bookingService.CreateAsync(newBooking);
            await _userService.UpdateAsync(currentUser);
            TempData["SuccessMessage"] = $"✅ Your booking for a cabin in {cabin.District.City.Name} / {cabin.District.Name} on {bookingStartTime:f} is confirmed.";
            return RedirectToAction("MyBookings");
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            var turkeyTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Turkey Standard Time" : "Europe/Istanbul";
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(turkeyTimeZoneId);
            var bookingsFromDb = await _bookingService.GetAllWithCabinForUserAsync(user.Id);

            var bookingViewModels = bookingsFromDb.Select(b =>
            {
                var startTimeUtc = DateTime.SpecifyKind(b.StartTime, DateTimeKind.Utc);
                var endTimeUtc = DateTime.SpecifyKind(b.EndTime, DateTimeKind.Utc);
                return new BookingViewModel
                {
                    Id = b.Id,
                    StartTime = TimeZoneInfo.ConvertTimeFromUtc(startTimeUtc, turkeyTimeZone),
                    EndTime = TimeZoneInfo.ConvertTimeFromUtc(endTimeUtc, turkeyTimeZone),
                    StartTimeUtc = startTimeUtc,
                    EndTimeUtc = endTimeUtc,
                    TotalPrice = b.Cabin.PricePerHour,
                    CabinLocation = $"{b.Cabin.District.City.Name} / {b.Cabin.District.Name}"
                };
            }).ToList();

            return View(bookingViewModels);
        }

        [HttpGet]
        public IActionResult CheckIn(string code)
        {
            if (string.IsNullOrEmpty(code)) { TempData["ErrorMessage"] = "QR scan failed: No data was received."; return RedirectToAction(nameof(MyBookings)); }
            TempData["SuccessMessage"] = $"✅ Successfully scanned code: {code}";
            return RedirectToAction(nameof(MyBookings));
        }
    }
}