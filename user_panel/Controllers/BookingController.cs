using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Data;
using user_panel.Helpers;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.LogServices;
using user_panel.ViewModels;
using System.Runtime.InteropServices;
using user_panel.Services.Firebase; // IFirebaseService için

namespace user_panel.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ICabinService _cabinService;
        private readonly IApplicationUserService _userService;
        private readonly IFirebaseService _firebaseService;

        public BookingController(
            IBookingService bookingService, 
            ICabinService cabinService, 
            IApplicationUserService userService,
            IFirebaseService firebaseService)
        {
            _bookingService = bookingService;
            _cabinService = cabinService;
            _userService = userService;
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            // Bu metot aynı kalıyor...
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
            // Bu metot aynı kalıyor...
            if (id == null) return NotFound();
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id.Value);
            if (cabin == null) return NotFound();
            return View(cabin);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id, DateTime? bookingDate)
        {
            // Bu metot aynı kalıyor...
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id);
            if (cabin == null) return NotFound();
            var date = (bookingDate.HasValue && bookingDate.Value.Date >= DateTime.Today) ? bookingDate.Value.Date : DateTime.Today;
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Turkey Standard Time" : "Europe/Istanbul");
            var startOfLocalDay = new DateTime(date.Year, date.Month, date.Day);
            var endOfLocalDay = startOfLocalDay.AddDays(1);
            var startOfUtcDay = TimeZoneInfo.ConvertTimeToUtc(startOfLocalDay, turkeyTimeZone);
            var endOfUtcDay = TimeZoneInfo.ConvertTimeToUtc(endOfLocalDay, turkeyTimeZone);
            var bookingsForDate = await _bookingService.GetWhereAsync(b => b.CabinId == id && b.StartTime >= startOfUtcDay && b.StartTime < endOfUtcDay);
            var viewModel = new CreateBookingViewModel
            {
                Cabin = cabin,
                BookingDate = date,
                BookedHours = bookingsForDate.Select(b => TimeZoneInfo.ConvertTimeFromUtc(b.StartTime, turkeyTimeZone).Hour).ToList(),
                MinBookingDate = DateTime.Today.ToString("yyyy-MM-dd")
            };
            return View(viewModel);
        }

        // --- TÜM HATALARI DÜZELTEN METOT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cabinId, DateTime bookingDate, int startTimeHour)
        {
            // 1. Gerekli verileri ve kullanıcıyı al
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(cabinId);
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (cabin == null || currentUser == null)
            {
                return NotFound();
            }

            // 2. Kontrolleri yap (geçmiş tarih, bakiye, çakışma)
            var bookingStartTime = bookingDate.Date.AddHours(startTimeHour);
            var bookingEndTime = bookingStartTime.AddHours(1);

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

            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Turkey Standard Time" : "Europe/Istanbul");
            var bookingStartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(bookingStartTime, turkeyTimeZone);
            var bookingEndTimeUtc = bookingStartTimeUtc.AddHours(1);

            var overlapping = await _bookingService.AnyAsync(b => b.CabinId == cabinId && bookingStartTimeUtc < b.EndTime && bookingEndTimeUtc > b.StartTime);

            if (overlapping)
            {
                TempData["ErrorMessage"] = "This time slot is already booked. Please choose another time.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            // 3. SQL veritabanına kaydet
            var newBooking = new Booking
            {
                ApplicationUserId = currentUser.Id,
                CabinId = cabinId,
                StartTime = bookingStartTimeUtc,
                EndTime = bookingEndTimeUtc,
                Cabin = cabin // Firebase servisine göndermek için gerekli
            };

            currentUser.CreditBalance -= bookingCost;
            await _bookingService.CreateAsync(newBooking);
            await _userService.UpdateAsync(currentUser);

            // 4. Firebase'e kaydet ve kullanıcıya sonuç mesajını göster
            try
            {
                await _firebaseService.CreateAccessPassAsync(newBooking);

                // --- DEĞİŞİKLİK BURADA ---
                // Hem SQL hem de Firebase başarılı olursa, bu mesaj gösterilecek.
                TempData["SuccessMessage"] = $"✅ Your booking for {bookingStartTime:h:00 tt} is confirmed!";
            }
            catch (Exception ex)
            {
                // Debug için hatayı konsola yazdır
                System.Diagnostics.Debug.WriteLine($"FIREBASE ERROR DETAILS: {ex.ToString()}");

                // --- DEĞİŞİKLİK BURADA ---
                // SQL başarılı ama Firebase başarısız olursa, bu uyarı mesajı gösterilecek.
                TempData["WarningMessage"] = $"Your booking is confirmed, but there was an issue creating the QR access pass. Please contact support.";
            }

            // 5. Sayfayı yeniden yönlendir
            return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            // Bu metot aynı kalıyor...
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
            // Bu metot aynı kalıyor...
            if (string.IsNullOrEmpty(code)) { TempData["ErrorMessage"] = "QR scan failed: No data was received."; return RedirectToAction(nameof(MyBookings)); }
            TempData["SuccessMessage"] = $"✅ Successfully scanned code: {code}";
            return RedirectToAction(nameof(MyBookings));
        }
    }
}