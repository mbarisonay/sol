using user_panel.Context;
using user_panel.Data;
using user_panel.Services.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // This using is needed for List<T>
using System.Linq;
using System.Threading.Tasks;

namespace user_panel.Services.Entity.CabinServices
{
    public class CabinService(ApplicationDbContext context) : EntityService<Cabin, int>(context), ICabinService
    {
        public async Task<List<Cabin>> GetCabinsWithLocationAsync()
        {
            return await _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .ToListAsync();
        }


        public async Task<Cabin?> GetCabinWithLocationByIdAsync(int cabinId)
        {
            return await _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .FirstOrDefaultAsync(c => c.Id == cabinId);
        }

        public async Task<List<Cabin>> SearchAsync(string searchTerm)
        {
            var query = _context.Cabins
                .Include(c => c.District)
                .ThenInclude(d => d.City)
                .AsQueryable();

            var sanitizedSearchTerm = searchTerm?.Trim().ToLower();

            if (string.IsNullOrEmpty(sanitizedSearchTerm))
            {
                var allCabins = await GetAllAsync();
                return allCabins.ToList();
            }

            query = query.Where(c =>
                c.Description.ToLower().Contains(sanitizedSearchTerm) ||
                c.District.Name.ToLower().Contains(sanitizedSearchTerm) ||
                c.District.City.Name.ToLower().Contains(sanitizedSearchTerm));

            return await query.ToListAsync();
        }
    }
}