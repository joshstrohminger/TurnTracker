import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DeleteActivityDialog } from './delete-activity.dialog';

describe('DeleteActivityComponent', () => {
  let component: DeleteActivityDialog;
  let fixture: ComponentFixture<DeleteActivityDialog>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DeleteActivityDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeleteActivityDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
