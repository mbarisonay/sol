using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user_panel.Data;
using user_panel.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using user_panel.Context;
using user_panel.Data;
using user_panel.Models;
using user_panel.ViewModels;

namespace user_panel.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // ===================================================================
        // ==> ADD THIS NEW METHOD FOR QR CODE VALIDATION
        // ===================================================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
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

            // 2. We assume the QR code contains the Cabin's ID as a number. Try to parse it.
            if (!int.TryParse(model.QrData, out int cabinId))
            {
                return BadRequest(new { success = false, message = "Invalid QR Code format. Expected a Cabin ID." });
            }

            // 3. Get the current time on the server to prevent client-side time manipulation
            var now = DateTime.UtcNow;

            // 4. Find an active reservation for this user and cabin at the current time
            //    This now works because `_context` is available.
            var validReservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.CabinId == cabinId &&
                    r.ReservationStartTime <= now &&
                    r.ReservationEndTime >= now);

            if (validReservation != null)
            {
                // SUCCESS: The user has a valid, active reservation for this cabin.
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

        // ===================================================================
        // YOUR EXISTING METHODS (UNCHANGED)
        // ===================================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreditBalance = 0
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("UserPanel", "Account");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserPanel()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // Include the user's reservations
            var userWithReservations = await _context.Users
                .Include(u => u.Reservations)
                .ThenInclude(r => r.Cabin)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            return View(userWithReservations.Reservations.OrderByDescending(r => r.ReservationStartTime));
        }

        // Placeholder for AddCredit to prevent errors from the button on UserPanel
        [HttpGet]
        [Authorize]
        public IActionResult AddCredit()
        {
            // You will need to create a simple View for this page.
            return View();
        }
    }
}
