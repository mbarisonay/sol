using Microsoft.AspNetCore.Identity;
using user_panel.Data;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public decimal CreditBalance { get; set; }
    public string? ProfilePicturePath { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];

    public ICollection<CabinReservation> CabinReservations { get; set; } = [];
}