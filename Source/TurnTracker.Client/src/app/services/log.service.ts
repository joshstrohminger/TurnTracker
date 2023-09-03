import { Injectable } from '@angular/core';
import { DateTime } from 'luxon';

export interface ISavedLog {
  time: DateTime;
  params: string[];
  level: LogLevel;
}

export enum LogLevel {
  unset,
  info,
  warn,
  error,
  debug
}

interface ILogServiceConfig {
  limit: number;
  enabled: boolean;
}

function getCircularReplacer() {
  const ancestors = [];
  return function (key, value) {
    if (typeof value !== "object" || value === null) {
      return value;
    }
    // `this` is the object that value is contained in, i.e., its direct parent.
    while (ancestors.length > 0 && ancestors.at(-1) !== this) {
      ancestors.pop();
    }
    if (ancestors.includes(value)) {
      return `[Circular to ${key} in level ${ancestors.lastIndexOf(value)}]`;
    }
    ancestors.push(value);
    return value;
  };
}

@Injectable({
  providedIn: 'root'
})
export class LogService {
  private readonly _storageKey = 'client-log-config';
  private _realConsoleLog: any;
  private _realConsoleError: any;
  private _realConsoleWarn: any;
  private _realConsoleInfo: any;
  private _realconsoleDebug: any;

  public readonly MinLimit = 1;
  public readonly MaxLimit = 1000;

  private readonly _logs: ISavedLog[] = [];
  public get logs(): ISavedLog[] {
    return this._logs;
  }

  private _limit = 100;
  public get limit(): number {
    return this._limit;
  }
  public set limit(value: number) {
    this._setLimit(value, true);
  }

  private _enabled = false;
  public get enabled(): boolean {
    return this._enabled;
  }
  public set enabled(value: boolean) {
    this._setEnabled(value, true);
  }

  constructor() {
    this._loadConfig();
  }

  public log(...params: any[]): void {
    console.log(...params);
    if (this._enabled) {
      this._log(LogLevel.unset, params);
    }
  }

  private _log(level: LogLevel, ...params: any[]): void {
    this._logs.push({
      level: level,
      time: DateTime.now(),
      params: params.map(param => {
        let json = JSON.stringify(param, getCircularReplacer(), 2);
        if (json === '{}') {
          const s = param.toString();
          if (s !== '[object Object]') {
            json = JSON.stringify(s);
          }
        }
        return json;
      })
    });

    if (this._logs.length > this._limit) {
      this._logs.shift();
    }
  }

  private _loadConfig() {
    const item = localStorage.getItem(this._storageKey);
    if (item) {
      try {
        const config = JSON.parse(item);
        if (config) {
          this._setEnabled(config.enabled, false);
          this._setLimit(config.limit, false);
        } else {
          localStorage.removeItem(this._storageKey);
        }
      } catch(e) {
        console.log('Failed to parse log service config', e)
        localStorage.removeItem(this._storageKey);
      }
    }
  }

  private _setLimit(limit: number, save: boolean) {
    if (typeof limit !== 'number') {
      limit = 0;
    }
    limit = Math.min(1000, Math.max(1, limit));

    if (limit === this._limit) {
      return;
    }

    this._limit = limit;

    if (save) {
      this._saveConfig();
    }

    if (this._logs.length > this._limit) {
      this._logs.splice(0, this._logs.length - this._limit);
    }
  }

  private _setEnabled(enabled: boolean, save: boolean) {
    if (typeof enabled !== 'boolean') {
      enabled = false;
    }

    if (this._enabled === enabled) {
      return;
    }

    this._enabled = enabled;

    if (save) {
      this._saveConfig();
    }

    if (this._enabled) {
      this._logs.length = 0;

      this._realConsoleLog = console.log;
      console.log = (...params: any[]) => this._log(LogLevel.unset, ...params);
      this._realConsoleError = console.error;
      console.error = (...params: any[]) => this._log(LogLevel.error, ...params);
      this._realConsoleInfo = console.info;
      console.info = (...params: any[]) => this._log(LogLevel.info, ...params);
      this._realConsoleWarn = console.warn;
      console.warn = (...params: any[]) => this._log(LogLevel.warn, ...params);
      this._realconsoleDebug = console.debug;
      console.debug = (...params: any[]) => this._log(LogLevel.debug, ...params);
    } else {
      console.log = this._realConsoleLog;
      console.error = this._realConsoleError;
      console.info = this._realConsoleInfo;
      console.warn = this._realConsoleWarn;
      console.debug = this._realconsoleDebug;
    }
  }

  private _saveConfig() {
    const config: ILogServiceConfig = {
      enabled: this._enabled,
      limit: this._limit
    };
    localStorage.setItem(this._storageKey, JSON.stringify(config))
  }
}
