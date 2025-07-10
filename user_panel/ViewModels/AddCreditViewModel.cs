using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels
{
    public class AddCreditViewModel
    {
        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(10, 5000, ErrorMessage = "Amount must be between ₺10 and ₺5,000.")]
        [Display(Name = "Amount to Add (₺)")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Cardholder Name")]
        public string CardHolderName { get; set; }

        [Required]
        [CreditCard]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Month must be in MM format (e.g., 05).")]
        [Display(Name = "Expiry Month (MM)")]
        public string ExpiryMonth { get; set; }

        [Required]
        [RegularExpression(@"^([2-9][0-9])$", ErrorMessage = "Year must be in YY format (e.g., 28).")]
        [Display(Name = "Expiry Year (YY)")]
        public string ExpiryYear { get; set; }

        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVC must be 3 or 4 digits.")]
        [Display(Name = "CVC")]
        public string CVC { get; set; }
    }
}