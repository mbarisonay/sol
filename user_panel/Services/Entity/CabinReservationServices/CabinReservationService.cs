using user_panel.Context;
using user_panel.Data;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.CabinReservationServices
{
    public class CabinReservationService(ApplicationDbContext context) : EntityService<CabinReservation, int>(context), ICabinReservationService
    {
    }
}
