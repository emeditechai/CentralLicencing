self.addEventListener('push', function (event) {
    if (!event.data) {
        return;
    }

    event.waitUntil((async function () {
        var payload = event.data.json();
        var windowClients = await self.clients.matchAll({
            type: 'window',
            includeUncontrolled: true
        });

        var hasVisibleClient = windowClients.some(function (client) {
            return client.visibilityState === 'visible' && client.focused;
        });

        if (payload.requireDocumentHidden !== false && hasVisibleClient) {
            var messageType = (payload.category && payload.category.indexOf('ticket-') === 0)
                ? 'ticket-notification'
                : 'expense-notification';
            await Promise.all(windowClients.map(function (client) {
                return client.postMessage({
                    type: messageType,
                    payload: payload
                });
            }));
            return;
        }

        await self.registration.showNotification(payload.title || 'Notification', {
            body: payload.message || 'A workflow update is available.',
            tag: payload.id || payload.category || 'expense-notification',
            renotify: true,
            data: {
                targetUrl: payload.targetUrl || '/Dashboard/Index'
            }
        });
    })());
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();

    event.waitUntil((async function () {
        var targetUrl = (event.notification.data && event.notification.data.targetUrl) || '/Dashboard/Index';
        var windowClients = await self.clients.matchAll({
            type: 'window',
            includeUncontrolled: true
        });

        for (var i = 0; i < windowClients.length; i += 1) {
            var client = windowClients[i];
            if ('focus' in client) {
                await client.focus();
                if ('navigate' in client) {
                    await client.navigate(targetUrl);
                }
                return;
            }
        }

        if (self.clients.openWindow) {
            await self.clients.openWindow(targetUrl);
        }
    })());
});