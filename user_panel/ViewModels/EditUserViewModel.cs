using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CreditBalance { get; set; }

        [Required]
        public string Role { get; set; } = null!;
    }

}
