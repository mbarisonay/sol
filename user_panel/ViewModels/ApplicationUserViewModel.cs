namespace user_panel.ViewModels
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string? UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public decimal CreditBalance { get; set; }

        public string Role { get; set; } = "Unknown";
    }

}
