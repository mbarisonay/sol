document.addEventListener('DOMContentLoaded', function () {

    // This function is called when a QR code is successfully scanned
    function onScanSuccess(decodedText, decodedResult) {
        // `decodedText` contains the data from the QR code (e.g., a booking ID or URL)
        console.log(`Scan result: ${decodedText}`);

        // Stop the scanner
        html5QrcodeScanner.clear();

        // Hide the modal manually
        var qrModal = bootstrap.Modal.getInstance(document.getElementById('qrScannerModal'));
        qrModal.hide();

        // --- ACTION: Redirect to a new page with the scanned data ---
        // You can create a new controller action to handle this.
        // For example, redirect to a check-in page:
        window.location.href = `/Booking/CheckIn?code=${decodedText}`;
    }

    // This function is called for scan errors (optional)
    function onScanError(errorMessage) {
        // handle scan error, usually you can ignore this.
        // console.warn(`QR Code scan error: ${errorMessage}`);
    }

    // Variable to hold the scanner instance
    let html5QrcodeScanner;
    const modalElement = document.getElementById('qrScannerModal');

    // --- Event listener for when the modal is SHOWN ---
    // We initialize the scanner only when the modal becomes visible.
    modalElement.addEventListener('shown.bs.modal', function () {
        if (!html5QrcodeScanner || !html5QrcodeScanner.isScanning) {
            html5QrcodeScanner = new Html5QrcodeScanner(
                "qr-reader", // The ID of the div to inject the scanner
                { fps: 10, qrbox: { width: 250, height: 250 } }, // Config
                /* verbose= */ false);

            html5QrcodeScanner.render(onScanSuccess, onScanError);
        }
    });

    // --- Event listener for when the modal is HIDDEN ---
    // We must stop the scanner and camera when the modal is closed.
    modalElement.addEventListener('hidden.bs.modal', function () {
        if (html5QrcodeScanner && html5QrcodeScanner.isScanning) {
            html5QrcodeScanner.clear().catch(error => {
                console.error("Failed to clear html5QrcodeScanner.", error);
            });
        }
    });
});