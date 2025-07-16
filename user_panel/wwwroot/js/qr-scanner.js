// This whole block will run once the page's HTML is ready.
document.addEventListener('DOMContentLoaded', function () {

    // --- SETUP ---
    // Get references to the HTML elements we need from _Layout.cshtml.
    const qrScannerModalElement = document.getElementById('qrScannerModal');

    // Check if the modal element exists before proceeding.
    if (!qrScannerModalElement) {
        console.error("Scanner Error: The modal with id 'qrScannerModal' was not found.");
        return;
    }

    // Create a new instance of the QR code scanner and tell it where to render the camera view.
    const html5QrCode = new Html5Qrcode("qr-reader");

    // --- FUNCTION DEFINITIONS ---
    // This function runs ONLY when a QR code is successfully scanned.
    const onScanSuccess = (decodedText, decodedResult) => {
        // Stop the camera to save power and prevent re-scans.
        stopScanner();

        // Get a reference to the Bootstrap modal instance and hide it.
        const modal = bootstrap.Modal.getInstance(qrScannerModalElement);
        modal.hide();

        // Send the scanned code to our backend controller: /Qr/VerifyQrCode
        fetch('/Qr/VerifyQrCode', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ qrCode: decodedText }),
        })
            .then(response => {
                // If the server says the code is not found (404), create a user-friendly error.
                if (!response.ok) {
                    throw new Error('This QR code is not valid for any cabin.');
                }
                return response.json(); // Get the cabin data (e.g., location) from the response.
            })
            .then(data => {
                // Success! We got the location from the server.
                const welcomeMessage = `Welcome to ${data.location} sports cabin. It's time to grind!`;
                alert(welcomeMessage); // Show the final message.
            })
            .catch(error => {
                // If anything went wrong (network error, invalid code), show an error alert.
                console.error('Verification Error:', error);
                alert(error.message);
            });
    };

    // A helper function to start the scanner.
    const startScanner = () => {
        html5QrCode.start(
            { facingMode: "environment" }, // Use the back camera on phones.
            { fps: 10, qrbox: { width: 250, height: 250 } },
            onScanSuccess,      // Function to call on success.
            (errorMessage) => { /* Ignore scan failures, keep trying. */ }
        ).catch(err => {
            console.error("Could not start QR scanner.", err);
            alert("Could not start camera. Please grant camera permissions.");
        });
    };

    // A helper function to safely stop the scanner.
    const stopScanner = () => {
        // Check if the scanner is actually running before trying to stop it.
        if (html5QrCode.isScanning) {
            html5QrCode.stop().catch(err => console.error("Error stopping scanner.", err));
        }
    };

    // --- EVENT LISTENERS (The "Magic") ---
    // Instead of listening for a button click, we listen for Bootstrap's own events.
    // This is more reliable.

    // Listen for when the modal is ABOUT TO BE SHOWN.
    qrScannerModalElement.addEventListener('show.bs.modal', function () {
        // Start the camera right when the modal opens.
        startScanner();
    });

    // Listen for when the modal has been COMPLETELY HIDDEN.
    qrScannerModalElement.addEventListener('hidden.bs.modal', function () {
        // Stop the camera when the modal closes, no matter how it was closed.
        stopScanner();
    });
});