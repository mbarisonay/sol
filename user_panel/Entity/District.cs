using user_panel.Data;

namespace user_panel.Entity
{
    public class District
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int CityId { get; set; }
        public City City { get; set; } = null!;

        public ICollection<Cabin> Cabins { get; set; } = [];
    }
}
