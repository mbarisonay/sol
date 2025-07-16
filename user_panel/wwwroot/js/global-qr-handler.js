document.addEventListener('DOMContentLoaded', function () {
    // This script handles the QR scanning functionality for the entire site.

    // Find the elements needed for scanning. These must exist in _Layout.cshtml.
    const qrScannerModalElement = document.getElementById('qrScannerModal');
    const qrResultModalElement = document.getElementById('qrResultModal');
    const qrResultDataElement = document.getElementById('qrResultData');

    // If the modals aren't on the page, stop the script.
    if (!qrScannerModalElement || !qrResultModalElement || !qrResultDataElement) {
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

    async function onScanSuccess(decodedText, decodedResult) {
        await html5QrcodeScanner.clear();
        qrScannerModal.hide();

        try {
            const response = await fetch('/api/access/unlock', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ qrCodeData: decodedText })
            });
            const result = await response.json();
            qrResultDataElement.textContent = result.message;
            qrResultModal.show();
        } catch (error) {
            qrResultDataElement.textContent = 'Could not verify access. Please try again.';
            qrResultModal.show();
        }
    }

    function onScanFailure(error) { /* This can be ignored */ }

    // Event listener for when the scanner modal is shown.
    qrScannerModalElement.addEventListener('shown.bs.modal', function () {
        if (!isScannerActive) {
            html5QrcodeScanner.render(onScanSuccess, onScanFailure);
            isScannerActive = true;
        }
    });

    // Event listener for when the scanner modal is hidden.
    qrScannerModalElement.addEventListener('hidden.bs.modal', function () {
        if (isScannerActive) {
            html5QrcodeScanner.clear().catch(err => { });
            isScannerActive = false;
        }
    });
});