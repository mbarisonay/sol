using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Configuration;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Helpers;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.CityServices;
using user_panel.Services.Entity.DistrictServices;
using user_panel.Services.Entity.LogServices;
using user_panel.ViewModels;

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
        private readonly ILogService _logService;
        private readonly Serilog.ILogger _logger;

        public AdminController(ILogService logService, IBookingService bookingService, IApplicationUserService userService, ICabinService cabinService, IDistrictService districtService, ICityService cityService, IConfiguration configuration)
        {
            _userService = userService;
            _cabinService = cabinService;
            _bookingService = bookingService;
            _districtService = districtService;
            _cityService = cityService;
            _logService = logService;
            _logger = LoggerHelper.GetManualLogger(configuration);
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
                _logger.Information("New cabin added by {User} with description: '{Description}', price per hour: '{PricePerHour}', district id: '{DistrictId}'",
                    User.Identity?.Name, newCabin.Description, newCabin.PricePerHour, newCabin.DistrictId);

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
        public async Task<IActionResult> GetCabinsByDistrict(int districtId)
        {
            var cabins = await _cabinService.GetCabinsByDistrictAsync(districtId);
            return PartialView("_UpdateCabinTable", cabins);
        }

        [HttpGet]
        public async Task<IActionResult> GetCabinsByCity(int cityId)
        {
            var cabins = await _cabinService.GetCabinsByCityAsync(cityId);
            return PartialView("_UpdateCabinTable", cabins);
        }



        [HttpGet]
        public async Task<IActionResult> UpdateCabin()
        {
            var cabins = await _cabinService.GetCabinsWithLocationAsync();
            ViewBag.Cities = (await _cityService.GetAllAsync())
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
            return View(cabins);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCabinsForFilter()
        {
            var cabins = await _cabinService.GetCabinsWithLocationAsync();
            return PartialView("_UpdateCabinTable", cabins);
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
                ViewBag.Cities = await _cityService.GetSelectListAsync();
                ViewBag.Districts = await _districtService.GetSelectListByCityIdAsync(model.CityId);
                return View(model);
            }

            var existingCabin = await _cabinService.GetCabinWithLocationByIdAsync(model.Id);
            if (existingCabin == null)
            {
                TempData["ErrorMessage"] = "Cabin not found.";
                return RedirectToAction("UpdateCabin");
            }

            string oldDescription = existingCabin.Description;
            decimal oldPrice = existingCabin.PricePerHour;
            int oldDistrictId = existingCabin.DistrictId;

            existingCabin.Description = model.Description;
            existingCabin.PricePerHour = model.PricePerHour;
            existingCabin.DistrictId = model.DistrictId;

            await _cabinService.UpdateAsync(existingCabin);
            _logger.Information("Cabin with id '{existingCabin}' has been edited from '{OldDescription}, '{OldPricePerHour}', '{OldDistrictId}' to '{Description}', '{PricePerHour}', '{DistrictId}' by {User}",
                existingCabin.Id, 
                oldDescription, oldPrice, oldDistrictId, 
                existingCabin.Description,existingCabin.PricePerHour, existingCabin.DistrictId, 
                User.Identity?.Name
                );


            TempData["StatusMessage"] = $"Cabin updated successfully!";
            return RedirectToAction("UpdateCabin");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCabin(int id)
        {
            var cabinToDelete = await _cabinService.GetCabinWithLocationByIdAsync(id);
            if (cabinToDelete == null)
            {
                TempData["StatusMessage"] = "❌ Error: Cabin not found for deletion.";
                return RedirectToAction("UpdateCabin");
            }

            await _cabinService.DeleteAsync(id);

            var cityName = cabinToDelete.District.City.Name;
            var districtName = cabinToDelete.District.Name;

            _logger.Information("Cabin with id {cabinId} has been deleted by {User}",
                cabinToDelete.Id, User.Identity?.Name);

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
            var oldUser = await _userService.GetUserForEditAsync(model.Id);
            var success = await _userService.UpdateUserAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return View(model);
            }

            _logger.Information("User with id {userId} is edited by {adminUser}. " +
                "Changes: First Name='{OldName}' -> '{NewName}', " +
                "Last Name='{OldSName}' -> '{NewSName}', " +
                "Email='{OldEmail}' -> '{NewEmail}', " +
                "Username='{OldUsername}' -> '{NewUsername}', " +
                "PhoneNum='{OldPhoneNum}' -> '{NewPhoneNum}', " +
                "Credit='{OldCredit}' -> '{NewCredit}', " +
                "Role='{OldRole}' -> '{NewRole}'",
                model.Id, User.Identity?.Name,
                oldUser.FirstName, model.FirstName,
                oldUser.LastName, model.LastName,
                oldUser.Email, model.Email,
                oldUser.UserName, model.UserName,
                oldUser.PhoneNumber, model.PhoneNumber,
                oldUser.CreditBalance, model.CreditBalance,
                oldUser.Role, model.Role
                );

            TempData["StatusMessage"] = "User updated successfully.";
            return RedirectToAction("ManageUser");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                TempData["StatusMessage"] = "Failed to delete user.";
            }
            TempData["StatusMessage"] = "User deleted successfully";

            _logger.Information("User with id '{userId}' has been deleted by '{User}'",
                id, User.Identity?.Name);

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
            _logger.Information("Booking with id '{bookingId}' has been deleted by '{User}'",
                id, User.Identity?.Name);
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBookingAndRefund(int id, string userId, decimal credit)
        {
            await _bookingService.DeleteAsync(id);
            await _userService.AddCreditAsync(userId, credit);
            TempData["StatusMessage"] = "Booking deleted successfully.";
            _logger.Information("Booking with id '{bookingId}' has been cancelled by '{User}' and user with id '{userId}' is refunded a balance of '{creditBalance}'",
                id, User.Identity?.Name, userId, credit);
            return RedirectToAction("ManageBookings");
        }

        [HttpGet]
        public async Task<IActionResult> ViewLogs()
        {
            var logs = await _logService.GetLogsAsync();
            return View(logs);
        }

    }
}
