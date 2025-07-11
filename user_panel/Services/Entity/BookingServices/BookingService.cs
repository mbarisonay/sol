using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using user_panel.Context;
using user_panel.Data;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.BookingServices
{
    public class BookingService(ApplicationDbContext context) : EntityService<Booking, int>(context), IBookingService
    {
        public async Task<IEnumerable<Booking>> GetAllWithCabinForUserAsync(string userId)
        {
            return await _dbSet
                .Where(b => b.ApplicationUserId == userId)
                .Include(b => b.Cabin)
                    .ThenInclude(c => c.District)
                    .ThenInclude(d => d.City)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _dbSet
                .Include(b => b.ApplicationUser)
                .Include(b => b.Cabin)
                    .ThenInclude(c => c.District)
                                        .ThenInclude(d => d.City)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<Booking, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<List<Booking>> GetWhereAsync(Expression<Func<Booking, bool>> predicate)
        {
            return await _context.Bookings
                .Where(predicate) // Apply the dynamic WHERE clause
                .ToListAsync();
        }
    }
}
