using System;
using System.ComponentModel.DataAnnotations;

namespace user_panel.Data
{
    public class CabinReservation
    {
        public int Id { get; set; }
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}