using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user_panel.Data;       
using user_panel.ViewModels; 
using System.Linq;
using System.Threading.Tasks;
using user_panel.Context;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.CabinReservationServices;
using user_panel.Services.Entity.CabinServices;
using System.Security.Claims;
using user_panel.Models;
using System;



namespace user_panel.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]
        [Authorize] // Ensures only logged-in users can call this
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> ValidateQrCode([FromBody] QrCodeScanModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.QrData))
            {
                return BadRequest(new { success = false, message = "Invalid QR code data." });
            }

            // 1. Get the current logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            // 2. We assume the QR code contains the Cabin's ID as a number.
            // Try to parse it.
            if (!int.TryParse(model.QrData, out int cabinId))
            {
                return BadRequest(new { success = false, message = "Invalid QR Code format. Expected a Cabin ID." });
            }

            // 3. Get the current time on the server to prevent client-side time manipulation
            var now = DateTime.UtcNow;

            // 4. Find an active reservation for this user and cabin at the current time
            var validReservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.CabinId == cabinId &&
                    r.ReservationStartTime <= now &&
                    r.ReservationEndTime >= now);

            if (validReservation != null)
            {
                // SUCCESS: The user has a valid, active reservation for this cabin.
                // In a real-world scenario, you would trigger the door unlocking mechanism here.
                return Ok(new { success = true, message = "Reservation confirmed! The door is now unlocked." });
            }
            else
            {
                // FAILURE: No matching reservation found.
                // Check if a reservation exists but for a different time to give a better error message.
                var futureReservation = await _context.Reservations
                    .AnyAsync(r => r.UserId == userId && r.CabinId == cabinId && r.ReservationStartTime > now);

                if (futureReservation)
                {
                    return BadRequest(new { success = false, message = "Access denied. Your reservation for this cabin is in the future. Please come back at the scheduled time." });
                }

                return BadRequest(new { success = false, message = "Access denied. No active reservation found for you at this cabin." });
            }
        }
        private readonly IApplicationUserService _userService;
        private readonly ICabinService _cabinService;

        public AccountController(IApplicationUserService userService, ICabinService cabinService)
        {
            _userService = userService;
            _cabinService = cabinService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userService.RegisterAsync(model);
            if (result.Succeeded) return RedirectToAction("Login");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userService.LoginAsync(model);
            if (result.Succeeded) return RedirectToAction("UserPanel");

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> UserPanel()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{User.Identity?.Name}'.");
            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userService.ChangePasswordAsync(user, model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["StatusMessage"] = "Your password has been changed.";
            return RedirectToAction("UserPanel");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateInformation()
        {
            var user = await _userService.GetCurrentUserAsync(User);

            if (user == null || user.Email == null || user.PhoneNumber == null)
                return NotFound("User or required fields missing.");

            var model = new UpdateInformationViewModel
            {
                NewEmail = user.Email,
                NewPhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmail(UpdateInformationViewModel model)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return NotFound();

            if (model.NewEmail != user.Email)
            {
                var result = await _userService.UpdateEmailAsync(user, model.NewEmail);
                if (result.Succeeded)
                    TempData["StatusMessage"] = "Your E-mail has been updated.";
            }
            return RedirectToAction("UserPanel");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePhoneNumber(UpdateInformationViewModel model)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return NotFound();

            if (model.NewPhoneNumber != user.PhoneNumber)
            {
                var result = await _userService.UpdatePhoneNumberAsync(user, model.NewPhoneNumber);
                if (result.Succeeded)
                    TempData["StatusMessage"] = "Your Phone Number has been updated.";
            }
            return RedirectToAction("UserPanel");
        }

        [HttpGet]
        public IActionResult AdminPanel()
        {
            ViewBag.AdminVerified = HttpContext.Session.GetString("AdminLoggedIn") == "true";
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(string username, string password)
        {
            if (username == "admin" && password == "1234")
            {
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                return RedirectToAction("AdminPanel");
            }

            TempData["LoginError"] = "Invalid admin credentials.";
            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public IActionResult LogoutAdmin()
        {
            HttpContext.Session.Remove("AdminLoggedIn");
            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public async Task<IActionResult> AddCabin(Cabin cabin)
        {
            if (!ModelState.IsValid) return View();

            await _cabinService.CreateAsync(cabin);
            return RedirectToAction("AdminPanel");
        }
    }
}