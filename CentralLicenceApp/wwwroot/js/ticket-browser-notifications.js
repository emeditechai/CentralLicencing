(function () {
    if (!window.signalR) {
        return;
    }

    var supportsNotificationApi = typeof window.Notification !== 'undefined';
    var supportsSecureWebPush = window.isSecureContext
        && supportsNotificationApi
        && 'serviceWorker' in navigator
        && 'PushManager' in window;
    var recentNotificationIds = new Map();
    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/ticket-notifications')
        .withAutomaticReconnect()
        .build();

    function getPermissionState() {
        if (!supportsNotificationApi) {
            return 'unsupported';
        }
        return window.Notification.permission;
    }

    function rememberNotification(id) {
        if (!id) {
            return false;
        }

        var now = Date.now();
        var existing = recentNotificationIds.get(id);
        if (existing && now - existing < 15000) {
            return true;
        }

        recentNotificationIds.set(id, now);
        Array.from(recentNotificationIds.keys()).forEach(function (key) {
            if (now - recentNotificationIds.get(key) > 60000) {
                recentNotificationIds.delete(key);
            }
        });

        return false;
    }

    function isDocumentActivelyVisible() {
        return document.visibilityState === 'visible' && document.hasFocus();
    }

    function showInAppToast(title, message, targetUrl) {
        if (!window.Swal) {
            return;
        }

        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'info',
            title: title,
            text: message,
            showConfirmButton: true,
            confirmButtonText: 'Open',
            timer: 9000,
            timerProgressBar: true,
            customClass: {
                popup: 'app-swal-popup',
                confirmButton: 'app-swal-confirm'
            },
            buttonsStyling: false
        }).then(function (result) {
            if (result.isConfirmed && targetUrl) {
                window.location.href = targetUrl;
            }
        });
    }

    function showBrowserNotification(payload) {
        var notification = new Notification(payload.title, {
            body: payload.message,
            tag: payload.id || payload.category || 'ticket-notification',
            renotify: true,
            requireInteraction: false,
            data: { targetUrl: payload.targetUrl }
        });

        notification.onclick = function (event) {
            event.preventDefault();
            window.focus();
            if (notification.data && notification.data.targetUrl) {
                window.location.href = notification.data.targetUrl;
            }
            notification.close();
        };
    }

    function handleTicketNotification(payload) {
        if (!payload) {
            return;
        }

        if (rememberNotification(payload.id)) {
            return;
        }

        var notificationPayload = {
            id: payload.id || '',
            title: payload.title || 'Ticket Update',
            message: payload.message || 'A support ticket requires your attention.',
            targetUrl: payload.targetUrl || '/HelpDeskTicket/Index',
            requireDocumentHidden: payload.requireDocumentHidden !== false
        };

        var canShowBrowserNotification = supportsNotificationApi
            && getPermissionState() === 'granted'
            && (!notificationPayload.requireDocumentHidden || !isDocumentActivelyVisible());

        if (canShowBrowserNotification) {
            showBrowserNotification(notificationPayload);
            return;
        }

        showInAppToast(notificationPayload.title, notificationPayload.message, notificationPayload.targetUrl);
    }

    // Listen for service worker forwarded push messages (ticket category)
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('message', function (event) {
            if (event.data && event.data.type === 'ticket-notification') {
                handleTicketNotification(event.data.payload);
            }
        });
    }

    async function startConnection() {
        try {
            await connection.start();
        } catch (_error) {
            window.setTimeout(startConnection, 5000);
        }
    }

    connection.on('TicketNotificationReceived', handleTicketNotification);

    startConnection();
})();
