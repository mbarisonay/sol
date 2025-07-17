using user_panel.Data;

namespace user_panel.Services.Firebase
{
    public interface IFirebaseService
    {
        Task CreateAccessPassAsync(Booking booking);
        Task DeleteAccessPassAsync(Booking booking);
    }
}