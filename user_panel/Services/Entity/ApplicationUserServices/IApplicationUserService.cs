using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using user_panel.Data;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.ApplicationUserServices
{
    public interface IApplicationUserService : IEntityService<ApplicationUser, int>
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, ChangePasswordViewModel model);
        Task<IdentityResult> UpdateEmailAsync(ApplicationUser user, string newEmail);
        Task<IdentityResult> UpdatePhoneNumberAsync(ApplicationUser user, string newPhoneNumber);
        string? GetCurrentUserId(ClaimsPrincipal userPrincipal);
    }
}
