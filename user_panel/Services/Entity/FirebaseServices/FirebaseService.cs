using Google.Cloud.Firestore;
using user_panel.Data;
using System;
using System.Threading.Tasks;

namespace user_panel.Services.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        // Alan aynı kalıyor.
        private readonly FirestoreDb _firestoreDb;

        // --- DEĞİŞİKLİK BURADA: CONSTRUCTOR ---
        // Artık bağlantıyı kendisi oluşturmaya çalışmıyor.
        // Dışarıdan, hazır ve kurulmuş bir FirestoreDb bağlantısı alıyor.
        public FirebaseService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task CreateAccessPassAsync(Booking booking)
        {
            if (booking.Cabin == null || string.IsNullOrEmpty(booking.Cabin.QrCode))
            {
                return;
            }

            var accessPassData = new
            {
                userId = booking.ApplicationUserId,
                gymId = booking.Cabin.QrCode,
                startTime = DateTime.SpecifyKind(booking.StartTime, DateTimeKind.Utc),
                endTime = DateTime.SpecifyKind(booking.EndTime, DateTimeKind.Utc)
            };

            string documentId = $"{booking.Cabin.QrCode}_{booking.ApplicationUserId}";

            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            await docRef.SetAsync(accessPassData);
        }

        // ... (DeleteAccessPassAsync metodu aynı kalıyor)
        public async Task DeleteAccessPassAsync(Booking booking)
        {
            if (booking.Cabin == null || string.IsNullOrEmpty(booking.Cabin.QrCode))
            {
                return;
            }

            string documentId = $"{booking.Cabin.QrCode}_{booking.ApplicationUserId}";

            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            await docRef.DeleteAsync();
        }
    }
}