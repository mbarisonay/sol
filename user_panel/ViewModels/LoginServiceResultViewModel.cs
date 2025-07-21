using Microsoft.AspNetCore.Identity;
namespace user_panel.ViewModels
{
    public class LoginServiceResultViewModel
    {
        public SignInResult SignInResult { get; set; }
        public string? RedirectAction { get; set; }
        public string? RedirectController { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
