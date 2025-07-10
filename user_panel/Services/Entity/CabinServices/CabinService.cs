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
        public async Task<List<Cabin>> SearchAsync(string searchTerm)
        {
            var query = _context.Set<Cabin>().AsQueryable();
            var sanitizedSearchTerm = searchTerm?.Trim().ToLower();

            if (string.IsNullOrEmpty(sanitizedSearchTerm))
            {
                // THIS IS THE FIX:
                // 1. Await the base method to get the IEnumerable<Cabin>.
                var allCabins = await base.GetAllAsync();
                // 2. Convert the IEnumerable<Cabin> to a List<Cabin> before returning.
                return allCabins.ToList();
            }

            query = query.Where(c =>
                c.Location.ToLower().Contains(sanitizedSearchTerm) ||
                c.Description.ToLower().Contains(sanitizedSearchTerm));

            return await query.ToListAsync();
        }
    }
}