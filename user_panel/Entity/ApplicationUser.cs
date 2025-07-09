using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace user_panel.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public decimal CreditBalance { get; set; }

        public ICollection<Booking> Bookings { get; set; } = [];

        public ICollection<CabinReservation> CabinReservations { get; set; } = [];
    }
}