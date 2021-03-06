import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { TakeTurnDialog } from './take-turn.dialog';

describe('TakeTurnDialog', () => {
  let component: TakeTurnDialog;
  let fixture: ComponentFixture<TakeTurnDialog>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TakeTurnDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TakeTurnDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
