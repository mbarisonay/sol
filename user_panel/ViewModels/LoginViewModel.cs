using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "E-mail or Phone Number")]
        public string EmailOrPhone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}