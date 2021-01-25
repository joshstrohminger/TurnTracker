import { TestBed } from '@angular/core/testing';

import { TitleContentService } from './title-content.service';

describe('TitleContentService', () => {
  let service: TitleContentService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TitleContentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
