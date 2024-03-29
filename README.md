# TurnTracker
![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/joshstrohminger/TurnTracker)
![GitHub last commit](https://img.shields.io/github/last-commit/joshstrohminger/TurnTracker)
![Azure Deploy](https://github.com/joshstrohminger/TurnTracker/workflows/Azure%20Deploy/badge.svg?branch=master)
![Dev Build](https://github.com/joshstrohminger/TurnTracker/workflows/Dev%20Build/badge.svg?branch=develop)

An attempt at a simple PWA version of an app to track turns for various chores, hosted on azure at www.turn-tracker.com

## Development
The server needs to be run to handle the back-end, but the front end can be run in two different ways.

### Auto Reload
The typical way of developing angular applications, using `ng serve --open` (also accessed using `npm start` and served at http://localhost:4200) will allow you to edit the front end quickly but will not provide access to any of the PWA features or operations because `ng serve` does not support running the PWA service worker.

### PWA
To test out the PWA features you can simply access the app from https://localhost:5001 or http://localhost:5000. This will be a locally hosted PWA and will serve the app that is in wwwroot, which is the output directory of the angular build.

### Building
`npm run build` will:

1. Update the build date by writing it to a file called _build.json_ to be used by the application's about page.
1. Run the angular production build.
1. Modify the service worker which gets rebuilt each time angular is built.

#### Service Worker
The service worker that angular provides for its built-in PWA support does not have the ability to customize how push notifications are handled, to reliably open the app, or to perform actions without opening the app. This is solved in a hacky way by simply replacing a chuck of the code in the service worker. This must be done after each build because the service worker gets rebuilt every time. The customized service work can now:

1. View a particular page when clicking the notification or a particular action on the notification.
1. Post to a provided URL without opening the app using a provided bearer token for authentication. This is used for snoozing and dismissing push notifications. 
1. Get rid of the notification without opening the app.

### Deployment

#### SQL Server DB

1. Allow connections from Azure resources.
1. Create a user (such as tim)
```sql
CREATE USER tim
	FOR LOGIN tim
	WITH DEFAULT_SCHEMA=[dbo]
GO
EXEC sp_addrolemember N'db_datareader', N'tim'
EXEC sp_addrolemember N'db_datawriter', N'tim'
EXEC sp_addrolemember N'db_ddladmin', N'tim'
GO
```

#### App Service

1. Update any necessary configuration values.
1. Update the DB connection string.