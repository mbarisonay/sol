using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace user_panel.Data
{
    public class Cabin
    {
        public int Id { get; set; }
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerHour { get; set; }

        public ICollection<Booking> Booking { get; set; } = [];
    }
}