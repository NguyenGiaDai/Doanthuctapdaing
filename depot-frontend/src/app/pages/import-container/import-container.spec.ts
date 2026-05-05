import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportContainer } from './import-container';

describe('ImportContainer', () => {
  let component: ImportContainer;
  let fixture: ComponentFixture<ImportContainer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportContainer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImportContainer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
