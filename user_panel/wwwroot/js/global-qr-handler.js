// wwwroot/js/global-qr-handler.js

document.addEventListener('DOMContentLoaded', function () {
    const qrScannerModalElement = document.getElementById('qrScannerModal');
    const qrResultModalElement = document.getElementById('qrResultModal');
    const qrResultTextElement = document.getElementById('qrResultText'); // Sonuç metni için bir element (örn. <p id="qrResultText"></p>)
    const qrResultTitleElement = document.getElementById('qrResultModalLabel'); // Modal başlığı için

    if (!qrScannerModalElement || !qrResultModalElement || !qrResultTextElement || !qrResultTitleElement) {
        console.error("Gerekli modal elementleri bulunamadı. QR Handler çalıştırılamıyor.");
        return;
    }

    const qrScannerModal = new bootstrap.Modal(qrScannerModalElement);
    const qrResultModal = new bootstrap.Modal(qrResultModalElement);

    let isScannerActive = false;
    const html5QrcodeScanner = new Html5QrcodeScanner(
        "qr-reader",
        { fps: 10, qrbox: { width: 250, height: 250 } },
        false
    );

    // Bu bizim Firebase'deki "Güvenlik Görevlimiz"
    const verifyAndOpenDoor = firebase.functions().httpsCallable('verifyAndOpenDoor');

    async function onScanSuccess(decodedText, decodedResult) {
        if (!isScannerActive) return; // Tarayıcı zaten kapandıysa tekrar işlem yapma

        // Tarayıcıyı durdur ve modal'ı gizle
        isScannerActive = false;
        await html5QrcodeScanner.clear();
        qrScannerModal.hide();

        // Kullanıcı ID'sini body'deki data-attribute'tan al
        const userId = document.body.getAttribute('data-user-id');
        if (!userId) {
            qrResultTitleElement.textContent = 'Hata';
            qrResultTextElement.textContent = 'Kullanıcı kimliği bulunamadı. Lütfen tekrar giriş yapın.';
            qrResultModal.show();
            return;
        }

        // Kullanıcıya beklemesini söyle
        qrResultTitleElement.textContent = 'Doğrulanıyor...';
        qrResultTextElement.textContent = 'Lütfen bekleyin, giriş izniniz kontrol ediliyor.';
        qrResultModal.show();

        try {
            // Firebase Cloud Function'ı çağır!
            const result = await verifyAndOpenDoor({
                gymId: decodedText, // QR koddan okunan salon ID'si
                userId: userId      // Giriş yapmış kullanıcının ID'si
            });

            // Başarılı sonuç geldi
            qrResultTitleElement.textContent = 'Başarılı!';
            qrResultTextElement.textContent = result.data.message; // "Kapı açılıyor!"
        } catch (error) {
            // Hatalı sonuç geldi
            qrResultTitleElement.textContent = 'Giriş Başarısız';
            qrResultTextElement.textContent = error.message || 'Bilinmeyen bir hata oluştu. Lütfen tekrar deneyin.';
        }
    }

    function onScanFailure(error) { /* Bu hata genellikle önemli değil, görmezden gelebiliriz. */ }

    qrScannerModalElement.addEventListener('shown.bs.modal', function () {
        if (!isScannerActive) {
            html5QrcodeScanner.render(onScanSuccess, onScanFailure);
            isScannerActive = true;
        }
    });

    qrScannerModalElement.addEventListener('hidden.bs.modal', function () {
        if (isScannerActive) {
            html5QrcodeScanner.clear().catch(err => { /* Hata olursa umursama */ });
            isScannerActive = false;
        }
    });
});