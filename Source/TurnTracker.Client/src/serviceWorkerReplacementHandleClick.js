notification.close();

console.log('received notification', notification);

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

const options = {};
