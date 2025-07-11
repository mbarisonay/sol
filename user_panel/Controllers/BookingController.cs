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

        // ===================================================================
        // === NEW "DETAILS" ACTION ADDED HERE ===
        // This action handles requests for the new details page.
        // It uses the existing ICabinService to fetch the data.
        // ===================================================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Use the injected service to get the cabin by its ID
            var cabin = await _cabinService.GetByIdAsync(id.Value);

            if (cabin == null)
            {
                return NotFound(); // Return 404 if no cabin with that ID exists
            }

            // Pass the found cabin object to the Details.cshtml view
            return View(cabin);
        }


        // === UPGRADED CREATE GET ACTION (Unchanged) ===
        [HttpGet]
        public async Task<IActionResult> Create(int id, DateTime? bookingDate)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id);
            if (cabin == null) return NotFound();

            var date = bookingDate.HasValue ? bookingDate.Value.Date : DateTime.Today;

            var bookingsForDate = await _bookingService.GetWhereAsync(b =>
                b.CabinId == id && b.StartTime.Date == date);

            var viewModel = new CreateBookingViewModel
            {
                Cabin = cabin,
                BookingDate = date,
                BookedHours = bookingsForDate.Select(b => b.StartTime.Hour).ToList()
            };

            return View(viewModel);
        }

        // --- UNCHANGED CREATE POST ACTION ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cabinId, DateTime bookingDate, int startTimeHour)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(cabinId); // District ve City dahil
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (cabin == null || currentUser == null)
                return NotFound();

            var bookingStartTime = bookingDate.Date.AddHours(startTimeHour);
            var bookingEndTime = bookingStartTime.AddHours(1);
            var bookingCost = cabin.PricePerHour;

            if (bookingStartTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Cannot book a time slot in the past.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            if (currentUser.CreditBalance < bookingCost)
            {
                TempData["ErrorMessage"] = $"Insufficient funds. Your balance is {currentUser.CreditBalance:C}, but the booking costs {bookingCost:C}.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            var overlapping = await _bookingService.AnyAsync(b =>
                b.CabinId == cabinId &&
                bookingStartTime < b.EndTime &&
                bookingEndTime > b.StartTime);

            if (overlapping)
            {
                TempData["ErrorMessage"] = "This time slot is already booked. Please choose another time.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
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

            var cityName = cabin.District.City.Name;
            var districtName = cabin.District.Name;

            TempData["SuccessMessage"] = $"✅ Your booking for a cabin in {districtName}, {cityName} on {bookingStartTime:f} is confirmed.";
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