import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TurnDetailsDialog } from './turn-details.dialog';

describe('TurnDetailsComponent', () => {
  let component: TurnDetailsDialog;
  let fixture: ComponentFixture<TurnDetailsDialog>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TurnDetailsDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TurnDetailsDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
