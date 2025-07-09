using Microsoft.AspNetCore.Identity;
namespace user_panel.ViewModels
{
    public class LoginServiceResultViewModel
    {
        public SignInResult SignInResult { get; set; }
        public string? RedirectAction { get; set; }
        public string? RedirectController { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
