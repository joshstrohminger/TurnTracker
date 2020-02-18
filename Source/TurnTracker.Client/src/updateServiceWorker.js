const fs = require('fs');
const path = require('path');

const contents = fs.readFileSync(path.join(__dirname, '..', 'angular.json'));
const angular = JSON.parse(contents);
const outputPath = angular["projects"]["turntracker-client"]["architect"]["build"]["options"]["outputPath"];
const worker = path.join(__dirname, '..', outputPath, 'ngsw-worker.js');

const codePath = path.join(__dirname, 'serviceWorkerReplacement.js');
const code = fs.readFileSync(codePath, 'utf8');
const codeLines = code.split(/\r?\n/).filter((line) => line.trim().length > 0);

function escapeRegExp(string) {
  return string.replace(/[.*+\-?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
}

const first = escapeRegExp(codeLines[0].trim());
const last = escapeRegExp(codeLines[codeLines.length-1].trim());
const pattern = first + '\\s+' + last;
console.log('pattern: ' + pattern);
const match = new RegExp(pattern);
match.pat

fs.readFile(worker, 'utf8', function (err,data) {
  if (err) {
    return console.error(err);
  }

  var result = data.replace(match, code);

  if (data.length === result.length) {
    return console.error('Failed to replace lines');
  }

  console.log(`Inserted lines`);

  fs.writeFile(worker, result, 'utf8', function (err) {
     if (err) return console.error(err);
  });
});
