using System;
using user_panel.Data;

namespace user_panel.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        // These properties will now hold the time already converted to Turkey's time zone for display.
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // --- NEW PROPERTIES ---
        // These will hold the original UTC times, purely for the status calculation logic.
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public decimal TotalPrice { get; set; }
        public string CabinLocation { get; set; }

        /// <summary>
        /// Calculates the current status of the booking in real-time.
        /// THIS LOGIC MUST ALWAYS USE UTC FOR ACCURACY.
        /// </summary>
        public string CurrentStatus
        {
            get
            {
                var now = DateTime.UtcNow;

                // We now compare against the explicit UTC properties.
                if (now >= StartTimeUtc && now < EndTimeUtc)
                {
                    return "Active";
                }
                if (now >= EndTimeUtc)
                {
                    return "Completed";
                }
                return "Upcoming";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (CurrentStatus)
                {
                    case "Active": return "bg-danger";
                    case "Completed": return "bg-secondary";
                    case "Upcoming": return "bg-success";
                    default: return "bg-info";
                }
            }
        }
    }
}