import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReommendationsList } from './recommendations-list';

describe('ReommendationsList', () => {
  let component: ReommendationsList;
  let fixture: ComponentFixture<ReommendationsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReommendationsList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReommendationsList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
