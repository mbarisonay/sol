using System;
using System.ComponentModel.DataAnnotations;

namespace user_panel.Data
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public int CabinId { get; set; }
        public Cabin Cabin { get; set; } = null!;

    }
}