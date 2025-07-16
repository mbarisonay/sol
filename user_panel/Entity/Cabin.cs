using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using user_panel.Entity;

namespace user_panel.Data
{
    public class Cabin
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal PricePerHour { get; set; }

        public int DistrictId { get; set; } // <-- null geçici olarak izin veriliyor
        public District District { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = [];
        
        [Display(Name = "QR Code")]
        public string? qr_code { get; set; }
    }
}