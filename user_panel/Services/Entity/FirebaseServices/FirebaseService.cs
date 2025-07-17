using FirebaseAdmin.Firestore;
using user_panel.Data;

namespace user_panel.Services.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseService()
        {
            // Proje ID'nizi buraya yazın. Bu ID, Firebase Proje Ayarları'nda bulunur.
            _firestoreDb = FirestoreDb.Create("kabin-sistemi");
        }

        public async Task CreateAccessPassAsync(Booking booking)
        {
            if (booking.Cabin == null || string.IsNullOrEmpty(booking.Cabin.QrCode))
            {
                // Hata yönetimi: Cabin veya QrCode bilgisi eksikse işlem yapma.
                // Belki burada bir loglama yapabilirsiniz.
                return;
            }

            var accessPassData = new
            {
                userId = booking.ApplicationUserId,
                gymId = booking.Cabin.QrCode, // SQL'deki Cabin.QrCode'u kullanıyoruz.
                startTime = DateTime.SpecifyKind(booking.StartTime, DateTimeKind.Utc),
                endTime = DateTime.SpecifyKind(booking.EndTime, DateTimeKind.Utc)
            };

            // Doküman ID'sini özel formatımızla oluşturuyoruz: {QrCode}_{UserId}
            string documentId = $"{booking.Cabin.QrCode}_{booking.ApplicationUserId}";
            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            // Bu "giriş izin belgesini" Firestore'a kaydediyoruz.
            await docRef.SetAsync(accessPassData);
        }

        public async Task DeleteAccessPassAsync(Booking booking)
        {
            if (booking.Cabin == null || string.IsNullOrEmpty(booking.Cabin.QrCode))
            {
                return;
            }

            string documentId = $"{booking.Cabin.QrCode}_{booking.ApplicationUserId}";
            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            // Rezervasyon iptal edildiğinde veya bittiğinde bu izin belgesini siliyoruz.
            await docRef.DeleteAsync();
        }
    }
}