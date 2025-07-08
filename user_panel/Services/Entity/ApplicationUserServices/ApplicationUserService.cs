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

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            ApplicationUser? user = model.EmailOrPhone.All(char.IsDigit)
                ? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhone)
                : await _userManager.FindByEmailAsync(model.EmailOrPhone);

            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return SignInResult.Failed;
            }

            return await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
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
