import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReloadComponent } from './reload.component';

describe('ReloadComponent', () => {
  let component: ReloadComponent;
  let fixture: ComponentFixture<ReloadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ReloadComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ReloadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
