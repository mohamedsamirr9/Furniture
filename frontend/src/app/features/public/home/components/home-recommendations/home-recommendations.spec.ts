import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeRecommendations } from './home-recommendations';

describe('HomeRecommendations', () => {
  let component: HomeRecommendations;
  let fixture: ComponentFixture<HomeRecommendations>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeRecommendations]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeRecommendations);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
