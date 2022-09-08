.forEach((name) => options[name] = desc[name]);
if (options.data && options.data.close && options.tag) {
  await this.scope.registration.getNotifications({ tag : options.tag }).then(function(notifications) {
    for(let notification of notifications) {
      notification.close();
    }
  });
  return;
}
await this.scope.registration.showNotification(desc["title"], options);
