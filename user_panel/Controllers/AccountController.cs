using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using user_panel.Helpers;
using user_panel.Models; // This should contain ApplicationUser
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.LogServices;
using user_panel.ViewModels;

namespace user_panel.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IApplicationUserService _userService;
        // THIS IS THE FIX: Changed IdentityUser to ApplicationUser
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private readonly Serilog.ILogger _logger;

        // THIS IS THE FIX: Changed the constructor parameter type
        public AccountController(IApplicationUserService userService, UserManager<ApplicationUser> userManager, ILogService logService, IConfiguration configuration)
        {
            _userService = userService;
            _userManager = userManager; // The rest of your code was correct to assume this
            _logService = logService;
            _logger = LoggerHelper.GetManualLogger(configuration);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _userService.RegisterAsync(model);
            if (result.Succeeded) 
            {
                _logger.Information("A new user has registered with full name '{Name} {Surname}', username '{Username}', email '{Email}' and phone number '{PhoneNum}'",
                    model.FirstName, model.LastName, model.UserName, model.Email, model.PhoneNumber);
                return RedirectToAction("Login"); 
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var serviceResult = await _userService.LoginAsync(model);
            if (serviceResult.SignInResult.Succeeded)
            {
                _logger.Information("User '{Username}' with email '{Email}' and ID '{UserId}' has logged in successfully",
                    serviceResult.Username, serviceResult.Email, serviceResult.UserId);
                return RedirectToAction(serviceResult.RedirectAction, serviceResult.RedirectController);
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
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessQrCode([FromBody] string qrCodeData)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found." });

            if (string.IsNullOrEmpty(qrCodeData))
            {
                return Json(new { success = false, message = "QR code data was empty." });
            }

            System.Diagnostics.Debug.WriteLine($"User '{user.UserName}' scanned QR Code with data: '{qrCodeData}'");

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

            _logger.Information("User '{Username}' (ID: '{UserId}') has changed their password",
                user.UserName, user.Email, user.Id);
            return RedirectToAction(nameof(UserPanel));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateInformation()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return NotFound();
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
            var oldEmail = user.Email;
            if (model.NewEmail != user.Email)
            {
                var result = await _userService.UpdateEmailAsync(user, model.NewEmail);
                if (result.Succeeded)
                    TempData["StatusMessage"] = "Your E-mail has been updated.";
                _logger.Information("User '{Username}' (ID: '{UserId}') updated their email to '{NewEmail}' from '{OldEmail}'",
                    user.UserName, user.Id, oldEmail, model.NewEmail);
            }
            return RedirectToAction(nameof(UserPanel));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePhoneNumber(UpdateInformationViewModel model)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return NotFound();
            var oldPhoneNum = user.PhoneNumber;
            if (model.NewPhoneNumber != user.PhoneNumber)
            {
                var result = await _userService.UpdatePhoneNumberAsync(user, model.NewPhoneNumber);
                if (result.Succeeded)
                    TempData["StatusMessage"] = "Your Phone Number has been updated.";
                _logger.Information("User '{Username}' (ID: '{UserId}') updated their phone number to '{NewPhone}' from '{OldPhone}'",
                    user.UserName, user.Id, oldPhoneNum, model.NewPhoneNumber);
            }
            return RedirectToAction(nameof(UserPanel));
        }

        [HttpGet]
        public IActionResult AddCredit()
        {
            return View(new AddCreditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCredit(AddCreditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetCurrentUserAsync(User);
                if (user == null) return Challenge();

                bool paymentSuccessful = true; // Simulate payment

                if (paymentSuccessful)
                {
                    var result = await _userService.AddCreditAsync(user, model.Amount);
                    if (result.Succeeded)
                    {
                        TempData["StatusMessage"] = $"Successfully added {model.Amount:C} to your account!";

                        _logger.Information("User '{Username}' (ID: '{UserId}') added a credit of '{Credit}'. New balance: {NewBalance} ",
                            user.UserName, user.Id, model.Amount, user.CreditBalance);

                        return RedirectToAction(nameof(UserPanel));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Payment could not be processed. Please try again.");
                }
            }

            return View(model);
        }
    }
}