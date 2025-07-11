using Microsoft.EntityFrameworkCore;
using user_panel.Data;

namespace user_panel.Context.Helpers
{
public static class ApplicationDbContextSeed
{
    public static void Seed(this ModelBuilder builder)
    {
        builder.Entity<Cabin>().HasData(
            //new Cabin
            //{
            //    Id = 1,
            //    Location = "Bornova/İzmir",
            //    Description = "A modern, compact cabin perfect for a high-intensity workout. Equipped with a smart treadmill and weight set.",
            //    PricePerHour = 25.00m
            //},
            //new Cabin
            //{
            //    Id = 2,
            //    Location = "Karşıyaka/İzmir",
            //    Description = "Spacious cabin with a focus on yoga and flexibility. Includes a full-length mirror and yoga mats.",
            //    PricePerHour = 25.00m
            //},
            //new Cabin
            //{
            //    Id = 3,
            //    Location = "Çankaya/Ankara",
            //    Description = "Premium cabin featuring a rowing machine and advanced monitoring systems for performance tracking.",
            //    PricePerHour = 30.00m
            //},
            //new Cabin
            //{
            //    Id = 4,
            //    Location = "Akyurt/Ankara",
            //    Description = "Standard cabin with essential cardio and strength training equipment. Great for a balanced workout.",
            //    PricePerHour = 20.00m
            //},
            //new Cabin
            //{
            //    Id = 5,
            //    Location = "Beşiktaş/İstanbul",
            //    Description = "An urban-style cabin with a boxing bag and a high-performance stationary bike.",
            //    PricePerHour = 35.00m
            //},
            //new Cabin
            //{
            //    Id = 6,
            //    Location = "Fatih/İstanbul",
            //    Description = "Historic district cabin offering a quiet and serene environment for mindful exercise and meditation.",
            //    PricePerHour = 30.00m
            //}
        );
    }
}

}
