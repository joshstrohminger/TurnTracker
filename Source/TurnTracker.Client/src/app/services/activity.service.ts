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
    // ensure updates occur in a separate change detection cycle
    setTimeout(() => this._isActive.next(value), 0);
  }

  constructor() { }
}
