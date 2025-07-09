using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user_panel.Data;       // Your ApplicationUser model
using user_panel.ViewModels; // Your ViewModels
using System.Linq;
using System.Threading.Tasks;
using user_panel.Context;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.CabinReservationServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.ViewModels;

namespace user_panel.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApplicationUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;
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

            var serviceResult = await _userService.LoginAsync(model);
            if (serviceResult.SignInResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(serviceResult.RedirectAction) && !string.IsNullOrEmpty(serviceResult.RedirectController))
                {
                    return RedirectToAction(serviceResult.RedirectAction, serviceResult.RedirectController);
                }
                else
                {
                    // Fallback, in case the service somehow didn't provide a redirect path on success
                    return RedirectToAction("UserPanel", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, serviceResult.ErrorMessage ?? "Invalid login attempt.");
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
            if (user == null) { 
            return NotFound($"Unable to load user with ID '{User.Identity?.Name}'.");
        }
            return View(user);
        }
        [Authorize]
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessQrCode([FromBody] string qrCodeData)
        {
            if (string.IsNullOrEmpty(qrCodeData))
            {
                return Json(new { success = false, message = "QR code data was empty." });
            }

            var user = await _userManager.GetUserAsync(User);

            // --- YOUR LOGIC GOES HERE ---
            // For now, we will just log it to the console for demonstration.
            // In a real application, you might:
            // - Look up the qrCodeData in a database.
            // - Assign an item to the user.
            // - Validate a ticket or pass.
            // - Redirect the user to a specific page.

            Console.WriteLine($"User '{user.UserName}' scanned QR Code with data: '{qrCodeData}'");

            // --- Example Response ---
            // You can return different data based on your logic.
            // For example, if the code is valid, you could return a redirect URL.
            return Json(new { success = true, message = "QR Code received successfully." });
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
    }
}