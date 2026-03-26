(function () {
    function initializeReceiptPreview(options) {
        var receiptInput = document.getElementById(options.inputId);
        var previewPlaceholder = document.getElementById(options.placeholderId);
        var previewContent = document.getElementById(options.contentId);

        if (!receiptInput || !previewPlaceholder || !previewContent) {
            return;
        }

        var existingReceipts = Array.isArray(options.existingReceipts) ? options.existingReceipts : [];
        var emptyMessage = options.emptyMessage || "Supports image and PDF files.";
        var currentObjectUrls = [];

        function revokePreviewUrls() {
            currentObjectUrls.forEach(function (objectUrl) {
                URL.revokeObjectURL(objectUrl);
            });
            currentObjectUrls = [];
        }

        function setPreviewMarkup(markup) {
            previewPlaceholder.classList.add('d-none');
            previewContent.classList.remove('d-none');
            previewContent.innerHTML = markup;
        }

        function setEmptyPreview(message) {
            previewContent.classList.add('d-none');
            previewContent.innerHTML = '';
            previewPlaceholder.classList.remove('d-none');

            var messageTarget = previewPlaceholder.querySelector('[data-preview-empty-message]');
            if (messageTarget) {
                messageTarget.textContent = message || emptyMessage;
            }
        }

        function buildImagePreview(url, fileName) {
            return [
                '<div class="w-100 d-flex flex-column" style="gap:.75rem;">',
                '<div style="font-size:.82rem;font-weight:700;color:#ffffff;word-break:break-word;">' + fileName + '</div>',
                '<img src="' + url + '" alt="' + fileName + '" style="width:100%;max-height:360px;object-fit:contain;border-radius:14px;background:#ffffff;padding:.35rem;" />',
                '</div>'
            ].join('');
        }

        function buildPdfPreview(url, fileName) {
            return [
                '<div class="w-100 d-flex flex-column" style="gap:.75rem;">',
                '<div style="font-size:.82rem;font-weight:700;color:#ffffff;word-break:break-word;">' + fileName + '</div>',
                '<iframe src="' + url + '" title="' + fileName + '" style="width:100%;height:360px;border:none;border-radius:14px;background:#ffffff;"></iframe>',
                '<a href="' + url + '" target="_blank" class="btn btn-light btn-sm align-self-center" style="border-radius:10px;">Open PDF in new tab</a>',
                '</div>'
            ].join('');
        }

        function buildFallbackPreview(url, fileName) {
            return [
                '<div class="d-flex flex-column align-items-center justify-content-center text-center" style="gap:.75rem;min-height:220px;">',
                '<i class="bi bi-file-earmark-text" style="font-size:2rem;color:#cbd5e1;"></i>',
                '<div style="font-size:.88rem;font-weight:600;word-break:break-word;">' + fileName + '</div>',
                '<a href="' + url + '" target="_blank" class="btn btn-light btn-sm" style="border-radius:10px;">Open Attachment</a>',
                '</div>'
            ].join('');
        }

        function buildPreviewCard(url, fileName) {
            var lowerName = (fileName || '').toLowerCase();
            if (lowerName.endsWith('.pdf')) {
                return buildPdfPreview(url, fileName);
            }

            if (lowerName.endsWith('.png') || lowerName.endsWith('.jpg') || lowerName.endsWith('.jpeg') || lowerName.endsWith('.webp') || lowerName.endsWith('.gif')) {
                return buildImagePreview(url, fileName);
            }

            return buildFallbackPreview(url, fileName);
        }

        function renderReceipts(receipts) {
            if (!receipts || !receipts.length) {
                setEmptyPreview(emptyMessage);
                return;
            }

            var markup = ['<div class="w-100 d-flex flex-column" style="gap:1rem;">'];
            receipts.forEach(function (receipt) {
                markup.push(buildPreviewCard(receipt.url, receipt.fileName));
            });
            markup.push('</div>');
            setPreviewMarkup(markup.join(''));
        }

        receiptInput.addEventListener('change', function (event) {
            revokePreviewUrls();
            var files = event.target.files ? Array.from(event.target.files) : [];
            if (!files.length) {
                renderReceipts(existingReceipts);
                return;
            }

            var selectedReceipts = files.map(function (file) {
                var objectUrl = URL.createObjectURL(file);
                currentObjectUrls.push(objectUrl);
                return {
                    url: objectUrl,
                    fileName: file.name
                };
            });

            renderReceipts(selectedReceipts);
        });

        renderReceipts(existingReceipts);

        window.addEventListener('beforeunload', revokePreviewUrls);
    }

    window.initializeReceiptPreview = initializeReceiptPreview;
})();