using System.Linq.Expressions;
using user_panel.Data;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.BookingServices
{
    public interface IBookingService : IEntityService<Booking, int>
    {
        Task<IEnumerable<Booking>> GetAllWithCabinForUserAsync(string userId);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<bool> AnyAsync(Expression<Func<Booking, bool>> predicate);
    }
}
