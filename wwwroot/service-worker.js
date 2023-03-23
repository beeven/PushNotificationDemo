console.log("Reporting from service-worker!")

self.addEventListener('push', (event)=>{
    event.waitUntil(self.registration.showNotification('From service worker',{
        body: 'Push Notification Subscription Management'
    }));
});

self.addEventListener('pushsubscriptionchange', (event)=>{
    console.log('Subscription expired');
    event.waitUntil(
        self.registration.pushManager.subscribe({ userVisibleOnly: true})
        .then(function(subscription){
            console.log('Subscribed after expiration', subscription.endpoint);
            return fetch('register', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ endpoint: subscription.endpoint})
            });
        })
    );
});



self.addEventListener('message', (event)=>{
    console.log(`Message received from ${event.origin}: ${event.data}`)
    event.source.postMessage("Hi client");
});