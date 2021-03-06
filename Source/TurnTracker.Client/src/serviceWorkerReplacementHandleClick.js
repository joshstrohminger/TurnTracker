notification.close();

yield;

var postUrl = notification.data['post-' + action];
if (postUrl) {
  console.log(`NotificationIntercept found post url: ${postUrl}`);
  var headers = new Headers();
  headers.set('Authorization', 'Bearer ' + notification.data['token-' + action]);
  headers.set('Content-Type', 'application/json');
  var body = JSON.stringify({when: new Date().toString()});
  var req = new Request(postUrl, {method: 'POST', headers: headers, body: body});
  try {
    this.scope.fetch(req);
  }
  catch (err) {
    console.error('Failed to post', err);
    this.debugger.log(err, 'NotificationIntercept.postError');
  }
  return;
}

var url = notification.data['view-' + action] || notification.data.url;
if (url) {
  console.log(`NotificationIntercept found view url: ${url}`);
  var found = false;
  var allClients = this.scope.clients;
  this.scope.clients.matchAll().then(function(clientsArr) {
    for (var i = 0; i < clientsArr.length; i++) {
      if (clientsArr[i].url === url) {
        // We already have a window to use, focus it.
        found = true;
        clientsArr[i].focus();
        console.log('NotificationIntercept focusing');
        break;
      }
    }
    if (!found) {
      // Create a new window.
      console.log('NotificationIntercept opening');
      allClients.openWindow(url).then(function(windowClient) {
        // do something with the windowClient.
      });
    }
  });
} else {
  console.log(`NotificationIntercept ignoring action: ${action}`);
}

const options = {};
