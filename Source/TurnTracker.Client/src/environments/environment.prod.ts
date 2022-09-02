import packageInfo from '../../package.json';
import buildInfo from './build.json';

export const environment = {
  production: true,
  version: String(packageInfo.version),
  appName: String(packageInfo.description),
  buildDate: String(buildInfo.buildDate)
};
