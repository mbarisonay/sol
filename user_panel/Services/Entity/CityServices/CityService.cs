using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.Services.Entity.CabinServices;

namespace user_panel.Services.Entity.CityServices
{
    public class CityService(ApplicationDbContext context) : EntityService<City, int>(context), ICityService
    {
        public async Task<List<SelectListItem>> GetSelectListAsync()
        {
            return await _context.Cities
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
        }
    }
}
