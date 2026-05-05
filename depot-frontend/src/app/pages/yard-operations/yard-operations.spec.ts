import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YardOperations } from './yard-operations';

describe('YardOperations', () => {
  let component: YardOperations;
  let fixture: ComponentFixture<YardOperations>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YardOperations],
    }).compileComponents();

    fixture = TestBed.createComponent(YardOperations);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
