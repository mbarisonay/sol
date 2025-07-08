document.addEventListener('DOMContentLoaded', function () {
    const scanButton = document.getElementById('scan-qr-button');
    const qrReaderContainer = document.getElementById('qr-reader-container');
    let html5QrCode = null;

    scanButton.addEventListener('click', function () {
        // Toggle the visibility of the QR reader container
        const isHidden = qrReaderContainer.style.display === 'none';
        qrReaderContainer.style.display = isHidden ? 'block' : 'none';

        if (isHidden) {
            // If we are showing the scanner, initialize and start it
            startQrScanner();
            scanButton.textContent = 'Cancel Scan'; // Update button text
        } else {
            // If we are hiding it, stop the scanner
            stopQrScanner();
            scanButton.textContent = 'Scan to Unlock Cabin'; // Reset button text
        }
    });

    function startQrScanner() {
        // Only create a new instance if one doesn't exist
        if (!html5QrCode) {
            html5QrCode = new Html5Qrcode("qr-reader");
        }

        const qrCodeSuccessCallback = (decodedText, decodedResult) => {
            // This function is called when a QR code is successfully scanned
            console.log(`Scan result: ${decodedText}`);

            // Stop the scanner immediately after a successful scan
            stopQrScanner();
            scanButton.textContent = 'Scan to Unlock Cabin'; // Reset button text
            qrReaderContainer.style.display = 'none';

            // Send the scanned data to the server for validation
            validateReservation(decodedText);
        };

        const config = { fps: 10, qrbox: { width: 250, height: 250 } };

        // Start scanning with the front camera
        html5QrCode.start({ facingMode: "environment" }, config, qrCodeSuccessCallback)
            .catch(err => {
                console.error("Unable to start scanning.", err);
                Swal.fire({
                    icon: 'error',
                    title: 'Camera Error',
                    text: 'Could not access the camera. Please ensure you have given permission.'
                });
            });
    }

    function stopQrScanner() {
        if (html5QrCode && html5QrCode.isScanning) {
            html5QrCode.stop().then(ignore => {
                console.log("QR Code scanning stopped.");
            }).catch(err => {
                console.error("Failed to stop the QR scanner.", err);
            });
        }
    }

    function validateReservation(qrData) {
        // Use fetch to send the QR data to our backend endpoint
        fetch('/Account/ValidateQrCode', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                // The RequestVerificationToken is needed to prevent CSRF attacks
                'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
            },
            body: JSON.stringify({ QrData: qrData })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Access Granted!',
                        text: data.message,
                        timer: 3000,
                        showConfirmButton: false
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Access Denied',
                        text: data.message
                    });
                }
            })
            .catch(error => {
                console.error('Error during validation:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Request Failed',
                    text: 'Could not communicate with the server. Please try again.'
                });
            });
    }
});