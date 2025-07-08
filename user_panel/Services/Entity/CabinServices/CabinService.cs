using user_panel.Context;
using user_panel.Data;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.CabinServices
{
    public class CabinService(ApplicationDbContext context) : EntityService<Cabin, int>(context), ICabinService
    {
    }
}
