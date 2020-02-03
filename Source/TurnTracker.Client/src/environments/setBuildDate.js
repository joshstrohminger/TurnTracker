const fs = require('fs');
const path = require('path');

const data = JSON.stringify({
  buildDate: new Date().toUTCString()
});

const filePath = path.join(__dirname, 'build.json');

fs.writeFileSync(filePath, data);
