/*
*
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

/* eslint-env browser, serviceworker, es6 */

'use strict';

/**
 * lang nghe su kien push
 * */

self.addEventListener('push', function(event) {
    console.log('[Service Worker] Push Received.');
    console.log(`[Service Worker] Push had this data: "${event.data.text()}"`);

    var jsonstr = event.data.text();
    var obj = JSON.parse(jsonstr);
    const title = obj.title;
    const options = {
        body: obj.msg,
        icon: obj.icon,
        url: obj.url,
        vibrate: [200, 100, 200, 100, 200, 100, 400],
        actions: [
            { action: obj.url, title: 'Open notice'},
            { action: 'close', title: 'Close' }]
     };
    // const options =
    // {
    //     "body": "Did you make a $1,000,000 purchase at Dr. Evil...",
    //     "icon": "images/icon.png",
    //     "vibrate": [200, 100, 200, 100, 200, 100, 400],
    //     "tag": "request",
    //     "actions": [
    //       { "action": "yes", "title": "Yes", "icon": "images/yes.png" },
    //       { "action": "no", "title": "No", "icon": "images/no.png" }
    //     ]
    //   };

    //event.waitUntil(self.registration.showNotification(title, options));
    const notificationPromise = self.registration.showNotification(title, options);
    event.waitUntil(notificationPromise);


  

  });

  /**
   * bat su kien click vao notify popup
   * 
   */
 
self.addEventListener('notificationclick', function (event) {

   
    //if (event.action === 'close') {
      
    //}
    console.log(event.action);
    //if (event.action === "" || event.action == null) {
    //    event.waitUntil(
    //        clients.openWindow("https://ims.enrichcous.com")
    //    );
    //}

    event.notification.close();
    if (event.action !== "" && event.action != null && event.action !== 'close') {
        event.waitUntil(
            clients.openWindow(event.action)
        );
    }
  

});

