using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using user_panel.Data;
using System.Threading.Tasks;

namespace user_panel.Services.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(FirestoreDb firestoreDb, ILogger<FirebaseService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task CreateAccessPassAsync(Booking booking)
        {
            // Gerekli nesnelerin null olup olmadığını kontrol edelim
            if (booking == null)
            {
                _logger.LogWarning("CreateAccessPassAsync: 'booking' nesnesi null geldi.");
                return;
            }
            if (booking.Cabin == null)
            {
                _logger.LogWarning("CreateAccessPassAsync: Booking nesnesi Cabin detaylarını içermiyor. Booking ID: {BookingId}", booking.Id);
                return;
            }
            if (string.IsNullOrEmpty(booking.Cabin.QrCode))
            {
                _logger.LogWarning("CreateAccessPassAsync: Cabin QrCode alanı boş veya null. Cabin ID: {CabinId}", booking.Cabin.Id);
                return;
            }

            var accessPassData = new
            {
                userId = booking.ApplicationUserId,
                gymId = booking.Cabin.QrCode, // Bu hala kabinin QR kodunu temsil ediyor, doğru.
                startTime = DateTime.SpecifyKind(booking.StartTime, DateTimeKind.Utc),
                endTime = DateTime.SpecifyKind(booking.EndTime, DateTimeKind.Utc)
            };

            // --- DEĞİŞİKLİK BURADA ---
            // Belge ID'si olarak SQL'deki Booking'in benzersiz ID'sini kullanıyoruz.
            // Bu, her rezervasyonun Firestore'da benzersiz bir kaydı olmasını sağlar.
            string documentId = booking.Id.ToString();
            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            _logger.LogInformation("Firestore'a belge oluşturuluyor: {DocumentId}", documentId);
            await docRef.SetAsync(accessPassData);
            _logger.LogInformation("Firestore belgesi başarıyla oluşturuldu: {DocumentId}", documentId);
        }

        // Rezervasyon silindiğinde veya iptal edildiğinde bu metot çağrılmalı.
        public async Task DeleteAccessPassAsync(Booking booking)
        {
            if (booking == null)
            {
                _logger.LogWarning("DeleteAccessPassAsync: 'booking' nesnesi null geldi.");
                return;
            }

            // --- DEĞİŞİKLİK BURADA ---
            // Silinecek belgenin ID'sini yine Booking ID'sinden alıyoruz.
            string documentId = booking.Id.ToString();
            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            _logger.LogInformation("Firestore'dan belge siliniyor: {DocumentId}", documentId);
            await docRef.DeleteAsync();
            _logger.LogInformation("Firestore belgesi başarıyla silindi: {DocumentId}", documentId);
        }
    }
}