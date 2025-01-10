/*
*
https://developers.google.com/web/fundamentals/codelabs/push-notifications/#handle_permission_denied

*  Push Notifications codelab
*  Copyright 2015 Google Inc. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      https://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License
*
*/

/* eslint-env browser, es6 */

'use strict';

//const pushButton = document.querySelector('.js-push-btn');

let isSubscribed = false;
let swRegistration = null;



/**B1: dang ky serviceworker */
if ('serviceWorker' in navigator && 'PushManager' in window) {
  console.log('Service Worker and Push is supported');

  navigator.serviceWorker.register('sw.js')
  .then(function(swReg) {
    console.log('Service Worker is registered', swReg);

    swRegistration = swReg;
  })
  .catch(function(error) {
    console.error('Service Worker Error', error);
  });
} else {
  console.warn('Push messaging is not supported');
  //pushButton.textContent = 'Push Not Supported';
}

//********** */

/**
 * B2:
 * publish key duoc dang ky truc tiep tu https://console.firebase.google.com/u/0/
 * Example
 * https://web-push-codelab.glitch.me/
 * 
 * Public Key
Note: You should never put your private key in your web app!

 */
//const applicationServerPublicKey = 'BAyQcxwpKM2tGDDnnyQgFJNNNhMvQZGCY0TcK1VZTVYmC92MOaoe2eDIUtxelwj58JSk1uAld3INMe0Bxp0MzZs';




function urlB64ToUint8Array(base64String) {
  const padding = '='.repeat((4 - base64String.length % 4) % 4);
  const base64 = (base64String + padding)
    .replace(/\-/g, '+')
    .replace(/_/g, '/');

  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);

  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}





/**
 * B3: kich hoat button enable push message
 */
function initializeUI() {

//  pushButton.addEventListener('click', function() {
//    pushButton.disabled = true;
//    if (isSubscribed) {
//      // TODO: Unsubscribe user
//      unsubscribeUser();
//    } else {
//      subscribeUser();
//    }

//  });

  // Set the initial subscription value
  swRegistration.pushManager.getSubscription()
  .then(function(subscription) {
    isSubscribed = !(subscription === null);

    updateSubscriptionOnServer(subscription);

    if (isSubscribed) {
      console.log('User IS subscribed.');
    } else {
        console.log('User is NOT subscribed.');

        subscribeUser();
    }

    //updateBtn();
  });
}


//thay doi trang thai button
//function updateBtn() {

//  if (Notification.permission === 'denied') {
//    pushButton.textContent = 'Push Messaging Blocked.';
//    pushButton.disabled = true;
//    updateSubscriptionOnServer(null);
//    return;
//  }


//  if (isSubscribed) {
//    pushButton.textContent = 'Disable Push Messaging';
//  } else {
//    pushButton.textContent = 'Enable Push Messaging';
//  }

//  pushButton.disabled = false;
//}

/**
 * B4: The last thing to do is call initializeUI() when our service worker is registered.
 */

navigator.serviceWorker.register('sw.js')
    .then(function (swReg) {
        console.log('Service Worker is registered', swReg);

        swRegistration = swReg;
        initializeUI();
    });

//luc nay user duoc thong bao la chua dang ky, 
// them vao initializeUI() function su kien click "Enable Push..." button de dang ky user
// va updateSubscriptionOnServer


/**
 * B5: When the user clicks the push button, we first disable the button just to make sure the user can't click it a second time while we're subscribing to push as it can take some time.
Then we call subscribeUser() when we know the user isn't currently 
 * 
 */
function subscribeUser() {
  const applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
  swRegistration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: applicationServerKey
  })
  .then(function(subscription) {
    console.log('User is subscribed.');

    updateSubscriptionOnServer(subscription);

    isSubscribed = true;

   // updateBtn();
  })
  .catch(function(err) {
    console.log('Failed to subscribe the user: ', err);
    //updateBtn();
  });
}

function unsubscribeUser() {
  swRegistration.pushManager.getSubscription()
  .then(function(subscription) {
    if (subscription) {
      return subscription.unsubscribe();
    }
  })
  .catch(function(error) {
    console.log('Error unsubscribing', error);
  })
  .then(function() {
    updateSubscriptionOnServer(null);

    console.log('User is unsubscribed.');
    isSubscribed = false;

    updateBtn();
  });
}


/**
 * 
 * Lets step through what this code is doing and how it's subscribing the user for push messaging.
First we take the application server's public key, which is base 64 URL safe encoded, and we convert it to a UInt8Array as this is the expected input of the subscribe call.
 We've already given you the function urlB64ToUint8Array at the top of scripts/main.js.
Once we've converted the value, we call the subscribe() method on our service worker's pushManager, passing in our application server's public key and the value userVisibleOnly: true.
 * 
 * 
 */

function updateSubscriptionOnServer(subscription) {
  // TODO: Send subscription to application server

  //const subscriptionJson = document.querySelector('.js-subscription-json');
  //const subscriptionDetails =
    //  document.querySelector('.js-subscription-details');

  //if (subscription) {
  //  subscriptionJson.textContent = JSON.stringify(subscription);
  //  subscriptionDetails.classList.remove('is-invisible');
  //} else {
  //  subscriptionDetails.classList.add('is-invisible');
  //}

    if (subscription) {
        console.log("Subscription: " + subscription);
        //console.log("Subscription: " + JSON.stringify(subscription));


        var req = new XMLHttpRequest();
        req.open("POST", "/home/NoticeSubscribe/", true);
        req.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        req.addEventListener('load', function () {
            console.log(req.status);
            console.log(req.responseText);
        });
        req.addEventListener('error', function () {
            console.log('Error occurred!');
        });
        //var jsonBody = {
        //    "code": JSON.stringify(subscription)
        //};
        req.send("code=" + JSON.stringify(subscription));
        console.log('Sent');

    }
    else {
        console.log("Subscription is NULL");
    }


}







