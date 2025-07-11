// user_panel/Data/Booking.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace user_panel.Data
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public int CabinId { get; set; }
        public Cabin Cabin { get; set; } // <-- The Cabin property is how we access the location

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public DateTime? CheckInTime { get; set; }

        // The incorrect 'Location' property has been removed.
    }
}