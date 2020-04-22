import { TestBed } from '@angular/core/testing';

import { ActivityInterceptor } from './activity.interceptor';

describe('ActivityInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      ActivityInterceptor
      ]
  }));

  it('should be created', () => {
    const interceptor: ActivityInterceptor = TestBed.inject(ActivityInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
