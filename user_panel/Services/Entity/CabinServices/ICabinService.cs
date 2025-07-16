using System.Collections.Generic; // <-- ADD THIS for List<T>
using System.Threading.Tasks;   // <-- ADD THIS for Task<>
using user_panel.Data;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.CabinServices
{
    public interface ICabinService : IEntityService<Cabin, int>
    {
        Task<List<Cabin>> GetCabinsWithLocationAsync();
        Task<Cabin?> GetCabinWithLocationByIdAsync(int cabinId);
        // This method definition is correct, it just needed the using statements above.
        Task<List<Cabin>> GetCabinsByDistrictAsync(int districtId);
        Task<List<Cabin>> GetCabinsByCityAsync(int cityId);
        Task<List<Cabin>> SearchAsync(string searchTerm);
        Task<Cabin?> GetCabinByQrCodeAsync(string qrCode);
    }
}