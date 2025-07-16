// Bu blok sayfa yüklendiğinde çalışır.
document.addEventListener('DOMContentLoaded', function () {

    const qrScannerModalElement = document.getElementById('qrScannerModal');
    if (!qrScannerModalElement) {
        console.error("Scanner Error: The modal with id 'qrScannerModal' was not found.");
        return;
    }

    const html5QrCode = new Html5Qrcode("qr-reader");

    // QR kodu başarıyla okunduğunda bu fonksiyon çalışır.
    const onScanSuccess = (decodedText, decodedResult) => {
        stopScanner();
        const modal = bootstrap.Modal.getInstance(qrScannerModalElement);
        modal.hide();

        // YENİ KONTROL METODUNU ÇAĞIRIYORUZ: /Qr/CheckInWithQr
        fetch('/Qr/CheckInWithQr', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ qrCode: decodedText }),
        })
            .then(response => {
                // Eğer sunucudan 404 (Not Found) gibi bir hata gelirse, bu bir sorundur.
                if (!response.ok) {
                    // Özel durum: Geçersiz QR kodu için sunucudan gelen mesajı kullanalım.
                    return response.json().then(err => { throw new Error(err.message || 'Invalid QR Code'); });
                }
                // Sunucudan gelen cevabı JSON formatına çevir.
                return response.json();
            })
            .then(data => {
                // ===================================================================
                // YENİ MANTIK: Gelen cevabın 'status' alanına göre karar ver.
                // ===================================================================
                if (data.status === 'success') {
                    // Başarılı giriş: Welcome mesajını göster.
                    const welcomeMessage = `Welcome to ${data.location} sports cabin. It's time to grind!`;
                    alert(welcomeMessage);
                } else if (data.status === 'no_reservation') {
                    // Rezervasyon yok: İlgili uyarıyı göster.
                    alert("You have no reservation");
                } else {
                    // Beklenmedik bir durum olursa genel bir hata göster.
                    alert("An unexpected error occurred.");
                }
            })
            .catch(error => {
                // Ağ hatası veya geçersiz QR gibi durumlarda hatayı göster.
                console.error('Verification Error:', error);
                alert(error.message); // Örn: "Invalid QR Code. Cabin not found."
            });
    };

    const startScanner = () => {
        html5QrCode.start(
            { facingMode: "environment" },
            { fps: 10, qrbox: { width: 250, height: 250 } },
            onScanSuccess,
            (errorMessage) => { /* Tarama hatalarını görmezden gel */ }
        ).catch(err => {
            console.error("Could not start QR scanner.", err);
            alert("Could not start camera. Please grant camera permissions.");
        });
    };

    const stopScanner = () => {
        if (html5QrCode.isScanning) {
            html5QrCode.stop().catch(err => console.error("Error stopping scanner.", err));
        }
    };

    // Modal açıldığında ve kapandığında kamerayı başlatan/durduran event listener'lar
    qrScannerModalElement.addEventListener('show.bs.modal', startScanner);
    qrScannerModalElement.addEventListener('hidden.bs.modal', stopScanner);
});