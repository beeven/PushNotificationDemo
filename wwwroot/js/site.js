// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function notifyMe() {
    if (!("Notification" in window)) {
        alert("This browser does not support desktop notification");
    } else if (Notification.permission == "granted") {
        const notification = new Notification("Hi there!");
    } else if (Notification !== "denied") {
        Notification.requestPermission().then((permission) => {
            if (permission == "granted") {
                const notification = new Notification("Hi there!");
            }
        });
    }
}

function postMessage() {
    navigator.serviceWorker.ready.then((registration)=>{
        registration.active.postMessage({"content":"I am a message from website.", "created": new Date()})
    })
}


var subscriptionButton = document.getElementById("subscriptionButton");

navigator.serviceWorker.register("service-worker.js", {scope: "/"}).then((registration)=>{
    let serviceWorker;
    if(registration.installing) {
        serviceWorker = registration.installing;
        document.querySelector("#kind").textContent = "installing";
    } else if (registration.waiting) {
        serviceWorker = registration.waiting;
        document.querySelector("#kind").textContent = "waiting";
    } else if (registration.active) {
        serviceWorker = registration.active;
        document.querySelector("#kind").textContent = "active"
    }
    if (serviceWorker) {
        document.querySelector("#state").textContent = serviceWorker.state;
        serviceWorker.addEventListener("statechange", (e)=>{
            document.querySelector("#state").textContent = serviceWorker.state;
        });
    }
}, (error)=>{
    console.log(error);
});

navigator.serviceWorker.onmessage = (event)=>{
    document.querySelector("#message").textContent = `The service worker sent me a message: ${event.data}`;
}

navigator.serviceWorker.ready
    .then((registration)=>{
        console.log("service worker registered");
        subscriptionButton.removeAttribute('disabled');
        return registration.pushManager.getSubscription();
    }).then((subscription)=>{
        if(subscription) {
            console.log('Already subscribed', subscription.endpoint);
            console.log(subscription);
            setUnsubscribeButton();
        } else {
            setSubscribeButton();
        }
    });

function subscribe() {
    navigator.serviceWorker.ready.then(async (registration)=>{
        const response = await fetch('/Subscription/VAPIDPublicKey');
        const vapidPublicKey = await response.text();
        // Chrome doesn't accept the base64-encoded (string) vapidPublicKey yet
        // urlBase64ToUint8Array() is defined in /tools.js
        const convertedVapidKey = urlBase64ToUint8Array(vapidPublicKey);
        return registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: convertedVapidKey
        });
    }).then((subscription)=>{
        console.log("Subscribed", subscription.endpoint);
        console.log(subscription);
        console.log(subscription.toJSON());
        let clientId = document.querySelector("#clientId").value ?? "beeven";
        return fetch('/Subscription/Register',{
            method: 'post',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                clientId: clientId,
                subscription: subscription
            })
        });
    }).then(setUnsubscribeButton);
}

function unsubscribe() {
    navigator.serviceWorker.ready
    .then((registration)=>{
        return registration.pushManager.getSubscription();
    }).then((subscription)=>{
        return subscription.unsubscribe()
            .then(()=>{
                console.log("Unsubscribed", subscription.endpoint);
                let clientId = document.querySelector("#clientId").value ?? "beeven";
                return fetch("/Subscription/Unregister",{
                    method: 'post',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ 
                        clientId: clientId,
                        subscription: subscription 
                    })
                });
            });
    }).then(setSubscribeButton);
}


function setSubscribeButton() {
    subscriptionButton.onclick = subscribe;
    subscriptionButton.textContent = 'Subscribe';
}

function setUnsubscribeButton() {
    subscriptionButton.onclick = unsubscribe;
    subscriptionButton.textContent = 'Unsubscribe';
}

function urlBase64ToUint8Array(base64String) {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding)
      .replace(/\-/g, '+')
      .replace(/_/g, '/');
   
    var rawData = window.atob(base64);
    var outputArray = new Uint8Array(rawData.length);
   
    for (var i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
  }