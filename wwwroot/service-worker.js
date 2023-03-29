console.log("Reporting from service-worker!")

console.log(self)

self.addEventListener('push', (event)=>{
    console.log(event);
    event.waitUntil(async function() {
        await self.registration.showNotification("Push notification",{
            body: "data: "+event.data?.text()
        });
        const allClients = await clients.matchAll({
            includeUncontrolled: true
        });
        for(const client of allClients) {
            console.log(client);
            client.postMessage(event.data?.text());
        }
    }());
    
    // console.log(self.Clients);
    // console.log(self.Client.postMessage(event.data));
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