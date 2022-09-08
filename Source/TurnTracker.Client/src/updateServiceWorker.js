/**
 * Attempt to replace a part of the service worker by matching the first and last lines of the replacement chunk to the original file.
 */

const fs = require('fs');
const path = require('path');

const contents = fs.readFileSync(path.join(__dirname, '..', 'angular.json'));
const angular = JSON.parse(contents);
const outputPath = angular["projects"]["turntracker-client"]["architect"]["build"]["options"]["outputPath"];
const worker = path.join(__dirname, '..', outputPath, 'ngsw-worker.js');

const green = '\x1b[32m%s\x1b[0m';
const red = '\x1b[31m%s\x1b[0m';

function escapeRegExp(string) {
  return string.replace(/[.*+\-?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
}

var data = fs.readFileSync(worker, {encoding: 'utf8'});

const names = ['serviceWorkerReplacementHandleClick.js', 'serviceWorkerReplacementHandlePush.js'];

for (const name of names) {
  console.log(`input: ${name}`);

  const codePath = path.join(__dirname, name)
  const code = fs.readFileSync(codePath, 'utf8');
  const codeLines = code.split(/\r?\n/).filter((line) => line.trim().length > 0);

  const first = escapeRegExp(codeLines[0].trim());
  const last = escapeRegExp(codeLines[codeLines.length-1].trim());

  const pattern = first + '\\s+' + last;
  console.log('pattern: ' + pattern);

  const match = new RegExp(pattern);
  const result = data.replace(match, code);

  if (data.length === result.length) {
    console.log(red, `Failed to replace lines for ${name}`);
    process.exit(1);
  }
  console.log(`inserted: ${codeLines.length - 2} lines\n`);
  data = result;
}

fs.writeFileSync(worker, data, {encoding: 'utf8'});
console.log(green, 'updated service worker');
