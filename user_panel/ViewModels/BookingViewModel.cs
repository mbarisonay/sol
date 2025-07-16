using System;
using user_panel.Data; // Add this to access the Cabin class

namespace user_panel.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string CabinLocation { get; set; } // e.g., "Ankara / Çankaya"

        /// <summary>
        /// Calculates the current status of the booking in real-time.
        /// This is the core logic for your new feature.
        /// </summary>
        public string CurrentStatus
        {
            get
            {
                // Use UtcNow for consistent server-side time comparison
                var now = DateTime.UtcNow;

                if (now >= StartTime && now < EndTime)
                {
                    return "Active";
                }
                if (now >= EndTime)
                {
                    return "Completed";
                }
                return "Upcoming";
            }
        }

        /// <summary>
        /// Determines the Bootstrap CSS class for the status badge
        /// based on the CurrentStatus property.
        /// </summary>
        public string StatusBadgeClass
        {
            get
            {
                switch (CurrentStatus)
                {
                    case "Active":
                        return "bg-danger"; // Red background for Active status
                    case "Completed":
                        return "bg-secondary"; // Gray background for Completed
                    case "Upcoming":
                        return "bg-success"; // Green background for Upcoming
                    default:
                        return "bg-info"; // A fallback color
                }
            }
        }
    }
}