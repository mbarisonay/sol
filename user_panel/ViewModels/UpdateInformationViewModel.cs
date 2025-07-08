using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels
{
    public class UpdateInformationViewModel
    {
        [EmailAddress]
        [Display(Name = "New E-mail")]
        public string NewEmail { get; set; }

        [Phone]
        [Display(Name = "New Phone Number")]
        public string NewPhoneNumber { get; set; }
    }
}