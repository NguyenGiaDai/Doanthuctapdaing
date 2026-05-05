import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YardMap } from './yard-map';

describe('YardMap', () => {
  let component: YardMap;
  let fixture: ComponentFixture<YardMap>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YardMap],
    }).compileComponents();

    fixture = TestBed.createComponent(YardMap);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
