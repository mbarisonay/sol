using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.Services.Entity.DistrictServices;

namespace user_panel.Services.Entity.DistrictServices
{
    public class DistrictService(ApplicationDbContext context) : EntityService<District, int>(context), IDistrictService
    {
        public async Task<List<SelectListItem>> GetSelectListByCityIdAsync(int cityId)
        {
            return await _context.Districts
                .Where(d => d.CityId == cityId)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToListAsync();
        }


        public async Task<List<SelectListItem>> GetDistrictSelectListAsync()
        {
            return await _context.Districts
                .Include(d => d.City)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Name} / {d.City.Name}"
                }).ToListAsync();
        }
    }
}
