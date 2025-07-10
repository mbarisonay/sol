using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using user_panel.Context;
using user_panel.Data;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.ApplicationUserServices
{
    public class ApplicationUserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager) : EntityService<ApplicationUser, int>(context), IApplicationUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                CreditBalance = 0
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                await _userManager.AddToRoleAsync(user, "Customer");
            }
            return result;
        }

        public async Task<LoginServiceResultViewModel> LoginAsync(LoginViewModel model)
        {
            ApplicationUser? user = model.EmailOrPhone.All(char.IsDigit)
                ? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhone)
                : await _userManager.FindByEmailAsync(model.EmailOrPhone);

            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return new LoginServiceResultViewModel { SignInResult = SignInResult.Failed, ErrorMessage = "Invalid login attempt." };
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (signInResult.Succeeded)
            {
                // Check roles and determine redirection path
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return new LoginServiceResultViewModel
                    {
                        SignInResult = signInResult,
                        RedirectAction = "Index",
                        RedirectController = "Admin"
                    };
                }
                else if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    return new LoginServiceResultViewModel
                    {
                        SignInResult = signInResult,
                        RedirectAction = "UserPanel",
                        RedirectController = "Account"
                    };
                }
                else
                {
                    // Default redirection if no specific role match
                    return new LoginServiceResultViewModel
                    {
                        SignInResult = signInResult,
                        RedirectAction = "UserPanel",
                        RedirectController = "Account"
                    };
                }
            }

            // If sign-in failed, return the failed result and a generic message
            return new LoginServiceResultViewModel { SignInResult = signInResult, ErrorMessage = "Invalid login attempt." };
        }

        public async Task<List<ApplicationUserViewModel>> GetAllUsersWithRolesAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<ApplicationUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Unknown";

                result.Add(new ApplicationUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    CreditBalance = user.CreditBalance,
                    Role = role
                });
            }

            return result;
        }

        public async Task<EditUserViewModel?> GetUserForEditAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            return new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                CreditBalance = user.CreditBalance,
                Role = role
            };
        }

        public async Task<bool> UpdateUserAsync(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.CreditBalance = model.CreditBalance;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();

            if (currentRole != model.Role)
            {
                if (currentRole != null)
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return true;
        }
        public async Task<IdentityResult> AddCreditAsync(ApplicationUser user, decimal amount)
        {
            if (user == null)
            {
                // Or throw an ArgumentNullException
                return IdentityResult.Failed(new IdentityError { Description = "User cannot be null." });
            }

            if (amount <= 0)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Credit amount must be positive." });
            }

            user.CreditBalance += amount;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userPrincipal)
        {
            return await _userManager.GetUserAsync(userPrincipal);
        }

        public string? GetCurrentUserId(ClaimsPrincipal userPrincipal)
        {
            return _userManager.GetUserId(userPrincipal);
        }


        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, ChangePasswordViewModel model)
        {
            return await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        }

        public async Task<IdentityResult> UpdateEmailAsync(ApplicationUser user, string newEmail)
        {
            user.Email = newEmail;
            user.UserName = newEmail;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdatePhoneNumberAsync(ApplicationUser user, string newPhoneNumber)
        {
            return await _userManager.SetPhoneNumberAsync(user, newPhoneNumber);
        }
    }

}
