import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ActivityService {

  private _isActive: BehaviorSubject<boolean> = new BehaviorSubject(true);
  public get isActive$() {
    return this._isActive.asObservable();
  }
  public set isActive(value: boolean) {
    this._isActive.next(value);
  }

  constructor() { }
}
