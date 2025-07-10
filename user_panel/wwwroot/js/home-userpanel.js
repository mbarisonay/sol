document.addEventListener('DOMContentLoaded', function () {

    // --- Element Selectors ---
    const scanQrButton = document.getElementById('scanQrBtn');
    const qrScannerModalElement = document.getElementById('qrScanModal');
    const qrResultModalElement = document.getElementById('qrResultModal');
    const qrResultDataElement = document.getElementById('qrResultData');

    // --- Defensive Check ---
    if (!scanQrButton || !qrScannerModalElement || !qrResultModalElement || !qrResultDataElement) {
        console.error('One or more required modal elements are missing from the page.');
        return;
    }

    // --- Bootstrap Modal Instances ---
    const qrScannerModal = new bootstrap.Modal(qrScannerModalElement);
    const qrResultModal = new bootstrap.Modal(qrResultModalElement);

    // --- State Management ---
    // These variables will help us manage the flow between modals.
    let scanSuccessful = false;
    let lastScannedData = '';

    // --- Helper Function to check for a valid URL ---
    function isUrl(str) {
        const pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
            '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name
            '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
            '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
            '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
            '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
        return !!pattern.test(str);
    }

    // --- QR Scanner Logic ---
    const html5QrcodeScanner = new Html5QrcodeScanner(
        "qr-reader",
        { fps: 10, qrbox: { width: 250, height: 250 } },
        false
    );

    function onScanSuccess(decodedText, decodedResult) {
        // When a scan is successful:
        // 1. Set our state flags.
        scanSuccessful = true;
        lastScannedData = decodedText;

        // 2. Hide the scanner modal. This will trigger the 'hidden.bs.modal' event.
        qrScannerModal.hide();
    }

    function onScanFailure(error) {
        // Ignore scan failures.
    }

    // --- Event Listeners ---

    // Listen for the "Scan QR" button click to start the process.
    scanQrButton.addEventListener('click', function () {
        qrScannerModal.show();
    });

    // When the SCANNER modal is SHOWN, start the camera.
    qrScannerModalElement.addEventListener('shown.bs.modal', function () {
        html5QrcodeScanner.render(onScanSuccess, onScanFailure);
    });

    // When the SCANNER modal is fully HIDDEN, decide what to do next.
    // THIS IS THE KEY TO FIXING THE LOOP.
    qrScannerModalElement.addEventListener('hidden.bs.modal', function () {
        // Always stop the scanner to turn off the camera.
        // .clear() is better than .stop() as it also removes the UI.
        html5QrcodeScanner.clear().catch(err => {
            // This might throw an error if the scanner is already cleared. It's safe to ignore.
        });

        // Check if the modal was closed BECAUSE of a successful scan.
        if (scanSuccessful) {
            // If the data is a URL, make it a clickable link.
            if (isUrl(lastScannedData)) {
                qrResultDataElement.innerHTML = `<a href="${lastScannedData}" target="_blank" rel="noopener noreferrer">${lastScannedData}</a>`;
            } else {
                // Otherwise, display it as plain text.
                qrResultDataElement.textContent = lastScannedData;
            }

            // Now that the first modal is gone, show the result modal.
            qrResultModal.show();

            // Reset the flag so this doesn't run if the user manually closes the result modal.
            scanSuccessful = false;
        }
    });

});