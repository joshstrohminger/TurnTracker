import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DangerDialog } from './danger.dialog';

describe('DangerDialog', () => {
  let component: DangerDialog;
  let fixture: ComponentFixture<DangerDialog>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DangerDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DangerDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
