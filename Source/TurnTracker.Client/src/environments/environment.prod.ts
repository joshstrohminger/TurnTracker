import { version, description } from '../../package.json';
import { buildDate } from './build.json';

export const environment = {
  production: true,
  version: String(version),
  appName: String(description),
  buildDate: String(buildDate)
};
