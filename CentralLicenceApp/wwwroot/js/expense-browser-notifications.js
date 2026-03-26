(function () {
    if (!window.signalR) {
        return;
    }

    var button = document.getElementById('browserNotificationToggle');
    var supportsNotificationApi = typeof window.Notification !== 'undefined';
    var supportsSecureWebPush = window.isSecureContext
        && supportsNotificationApi
        && 'serviceWorker' in navigator
        && 'PushManager' in window;
    var antiForgeryTokenMeta = document.querySelector('meta[name="request-verification-token"]');
    var antiForgeryToken = antiForgeryTokenMeta ? antiForgeryTokenMeta.getAttribute('content') : '';
    var recentNotificationIds = new Map();
    var serviceWorkerRegistration = null;
    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/expense-notifications')
        .withAutomaticReconnect()
        .build();

    function getPermissionState() {
        if (!supportsNotificationApi) {
            return 'unsupported';
        }

        return window.Notification.permission;
    }

    function updateButtonState() {
        if (!button) {
            return;
        }

        var permission = getPermissionState();
        button.classList.remove('is-enabled', 'is-blocked', 'is-live-only');

        if (!supportsSecureWebPush) {
            button.innerHTML = '<i class="bi bi-broadcast-pin me-1"></i> Live Alerts Only';
            button.classList.add('is-live-only');
            button.disabled = false;
            return;
        }

        if (permission === 'granted') {
            button.innerHTML = '<i class="bi bi-bell-fill me-1"></i> Alerts On';
            button.classList.add('is-enabled');
            button.disabled = false;
            return;
        }

        if (permission === 'denied') {
            button.innerHTML = '<i class="bi bi-bell-slash me-1"></i> Alerts Blocked';
            button.classList.add('is-blocked');
            button.disabled = false;
            return;
        }

        if (permission === 'unsupported') {
            button.innerHTML = '<i class="bi bi-bell-slash me-1"></i> Alerts Unsupported';
            button.classList.add('is-blocked');
            button.disabled = true;
            return;
        }

        button.innerHTML = '<i class="bi bi-bell me-1"></i> Enable Alerts';
    }

    function urlBase64ToUint8Array(base64String) {
        var padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        var base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        var rawData = window.atob(base64);
        var outputArray = new Uint8Array(rawData.length);

        for (var index = 0; index < rawData.length; ++index) {
            outputArray[index] = rawData.charCodeAt(index);
        }

        return outputArray;
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
            tag: payload.id || payload.category || 'expense-request-submitted',
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

    function handleExpenseRequestSubmitted(payload) {
        if (!payload) {
            return;
        }

        if (rememberNotification(payload.id)) {
            return;
        }

        var notificationPayload = {
            id: payload.id || '',
            title: payload.title || 'Expense request submitted',
            message: payload.message || 'A new expense or advance request requires attention.',
            targetUrl: payload.targetUrl || '/Dashboard/Index',
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

    async function fetchPushConfig() {
        var response = await fetch('/push-notifications/public-key', {
            credentials: 'same-origin'
        });

        if (!response.ok) {
            throw new Error('Unable to load push notification settings.');
        }

        return response.json();
    }

    async function registerServiceWorker() {
        if (!supportsSecureWebPush) {
            return null;
        }

        if (serviceWorkerRegistration) {
            return serviceWorkerRegistration;
        }

        serviceWorkerRegistration = await navigator.serviceWorker.register('/service-worker.js');
        navigator.serviceWorker.addEventListener('message', function (event) {
            if (event.data && event.data.type === 'expense-notification') {
                handleExpenseRequestSubmitted(event.data.payload);
            }
        });

        return serviceWorkerRegistration;
    }

    async function saveSubscription(subscription) {
        var subscriptionJson = subscription.toJSON();
        await fetch('/push-notifications/subscribe', {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({
                endpoint: subscriptionJson.endpoint,
                p256dh: subscriptionJson.keys.p256dh,
                auth: subscriptionJson.keys.auth
            })
        });
    }

    async function ensurePushSubscription() {
        if (!supportsSecureWebPush) {
            updateButtonState();
            return;
        }

        var pushConfig = await fetchPushConfig();
        if (!pushConfig.enabled || !pushConfig.publicKey) {
            button.innerHTML = '<i class="bi bi-bell-slash me-1"></i> Alerts Unavailable';
            button.classList.add('is-blocked');
            return;
        }

        var registration = await registerServiceWorker();
        if (!registration) {
            return;
        }

        var existingSubscription = await registration.pushManager.getSubscription();
        if (existingSubscription) {
            await saveSubscription(existingSubscription);
            updateButtonState();
            return;
        }

        if (window.Notification.permission !== 'granted') {
            updateButtonState();
            return;
        }

        var subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(pushConfig.publicKey)
        });

        await saveSubscription(subscription);
        updateButtonState();
    }

    async function requestPermission() {
        if (!supportsSecureWebPush) {
            updateButtonState();
            return;
        }

        try {
            var permission = await window.Notification.requestPermission();
            if (permission === 'granted') {
                await ensurePushSubscription();
            }
        } finally {
            updateButtonState();
        }
    }

    async function startConnection() {
        try {
            await connection.start();
        } catch (_error) {
            window.setTimeout(startConnection, 5000);
        }
    }

    connection.on('ExpenseNotificationReceived', handleExpenseRequestSubmitted);

    if (button) {
        button.addEventListener('click', function () {
            if (!supportsSecureWebPush) {
                showInAppToast(
                    'Live alerts enabled',
                    'HTTP can use live in-app alerts while the browser session is open. Closed-browser web push requires HTTPS or localhost secure context.',
                    '/Dashboard/Index');
                return;
            }

            if (getPermissionState() !== 'granted') {
                requestPermission();
                return;
            }

            ensurePushSubscription();
        });
    }

    document.addEventListener('visibilitychange', updateButtonState);
    window.addEventListener('focus', updateButtonState);
    window.addEventListener('blur', updateButtonState);

    updateButtonState();
    ensurePushSubscription().catch(function () {
        updateButtonState();
    });
    startConnection();
})();