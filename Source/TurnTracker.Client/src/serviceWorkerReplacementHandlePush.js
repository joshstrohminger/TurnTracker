.forEach(name => options[name] = desc[name]);
if (options.data && options.data.close && options.tag) {
    yield this.scope.registration.getNotifications({ tag : options.tag }).then(function(notifications) {
        for(let notification of notifications) {
            notification.close();
        }
    });
    return;
}
yield this.scope.registration.showNotification(desc['title'], options);
