using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels
{
    public class CabinInfoViewModel
    {
        public int Id { get; set; }

        public int CityId { get; set; }

        public int DistrictId { get; set; }

        public string Description { get; set; } = null!;

        public decimal PricePerHour { get; set; }
    }
}