using Microsoft.AspNetCore.Mvc.Rendering;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.DistrictServices
{
    public interface IDistrictService : IEntityService<District, int>
    {
        Task<List<SelectListItem>> GetSelectListByCityIdAsync(int cityId);
        Task<List<SelectListItem>> GetDistrictSelectListAsync();
    }
}
