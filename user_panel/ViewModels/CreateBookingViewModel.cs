using user_panel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels
{
    public class CreateBookingViewModel
    {
        // Details of the cabin being booked
        public Cabin Cabin { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Today;
        public List<int> BookedHours { get; set; } = new List<int>();
        public string MinBookingDate { get; set; }
    }
}