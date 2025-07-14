using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Data;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.ViewModels;
using user_panel.Services.Entity.DistrictServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using user_panel.Services.Entity.CityServices;

namespace user_panel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICityService _cityService;
        private readonly IApplicationUserService _userService;
        private readonly ICabinService _cabinService;
        private readonly IBookingService _bookingService;
        private readonly IDistrictService _districtService;

        public AdminController(IBookingService bookingService, IApplicationUserService userService, ICabinService cabinService, IDistrictService districtService, ICityService cityService)
        {
            _userService = userService;
            _cabinService = cabinService;
            _bookingService = bookingService;
            _districtService = districtService;
            _cityService = cityService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddCabin()
        {
            ViewBag.Cities = (await _cityService.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddCabin(CabinInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newCabin = new Cabin
                {
                    Description = model.Description,
                    PricePerHour = model.PricePerHour,
                    DistrictId = model.DistrictId
                };

                await _cabinService.CreateAsync(newCabin);
                TempData["StatusMessage"] = "Cabin added successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Cities = (await _cityService.GetAllAsync())
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistrictsByCity(int id)
        {
            var districts = await _districtService.GetSelectListByCityIdAsync(id);
            return Json(districts);
        }




        [HttpGet]
        public async Task<IActionResult> UpdateCabin()
        {
            var cabins = await _cabinService.GetCabinsWithLocationAsync();
            return View(cabins);
        }

        [HttpGet]
        public async Task<IActionResult> EditCabin(int id)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id);

            if (cabin == null)
            {
                TempData["StatusMessage"] = "Error: Cabin not found!";
                return RedirectToAction("UpdateCabin");
            }

            var model = new CabinInfoViewModel
            {
                Id = cabin.Id,
                Description = cabin.Description,
                PricePerHour = cabin.PricePerHour,
                CityId = cabin.District.CityId,
                DistrictId = cabin.DistrictId
            };

            ViewBag.Cities = await _cityService.GetSelectListAsync();
            ViewBag.Districts = await _districtService.GetSelectListByCityIdAsync(model.CityId);

            return View(model); // ✅ doğru ViewModel döndürülüyor
        }


        [HttpPost]
        public async Task<IActionResult> EditCabin(CabinInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ViewBag tekrar doldurulmalı
                ViewBag.Cities = await _cityService.GetSelectListAsync();
                ViewBag.Districts = await _districtService.GetSelectListByCityIdAsync(model.CityId);
                return View(model);
            }

            var existingCabin = await _cabinService.GetByIdAsync(model.Id);
            if (existingCabin == null)
            {
                TempData["ErrorMessage"] = "Cabin not found.";
                return RedirectToAction("UpdateCabin");
            }

            existingCabin.Description = model.Description;
            existingCabin.PricePerHour = model.PricePerHour;
            existingCabin.DistrictId = model.DistrictId;

            await _cabinService.UpdateAsync(existingCabin);

            TempData["StatusMessage"] = $"Cabin updated successfully!";
            return RedirectToAction("UpdateCabin");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCabin(int id)
        {
            var cabinToDelete = await _cabinService.GetCabinWithLocationByIdAsync(id); // District ve City içeren haliyle al
            if (cabinToDelete == null)
            {
                TempData["StatusMessage"] = "❌ Error: Cabin not found for deletion.";
                return RedirectToAction("UpdateCabin");
            }

            await _cabinService.DeleteAsync(id);

            var cityName = cabinToDelete.District.City.Name;
            var districtName = cabinToDelete.District.Name;

            TempData["StatusMessage"] = $"✅ Cabin in '{districtName}, {cityName}' was successfully deleted.";
            return RedirectToAction("UpdateCabin");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUser(string? search)
        {
            var user = await _userService.GetAllUsersWithRolesAsync(search);
            ViewBag.SearchQuery = search;
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserPartial(string? search)
        {
            var users = await _userService.GetAllUsersWithRolesAsync(search);
            return PartialView("_ManageUserTable", users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var model = await _userService.GetUserForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await _userService.UpdateUserAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return View(model);
            }

            TempData["StatusMessage"] = "User updated successfully.";
            return RedirectToAction("ManageUser");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _userService.DeleteUserAsync(id);
            TempData["StatusMessage"] = success
                ? "User deleted successfully."
                : "Failed to delete user.";
            return RedirectToAction("ManageUser");
        }

        [HttpGet]
        public async Task<IActionResult> ManageBookings()
        {
            var result = await _bookingService.GetAllBookingsAsync();
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            await _bookingService.DeleteAsync(id);
            TempData["StatusMessage"] = "Booking deleted successfully.";
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBookingAndRefund(int id, string userId, decimal credit)
        {
            await _bookingService.DeleteAsync(id);
            await _userService.AddCreditAsync(userId, credit);
            TempData["StatusMessage"] = "Booking deleted successfully.";
            return RedirectToAction("ManageBookings");
        }
    }
}
