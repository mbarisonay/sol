using Microsoft.AspNetCore.Mvc.Rendering;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.CityServices
{
    public interface ICityService : IEntityService<City, int>
    {
        Task<List<SelectListItem>> GetSelectListAsync();

    }
}
