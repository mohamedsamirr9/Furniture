import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Bestseller } from './bestseller';

describe('Bestseller', () => {
  let component: Bestseller;
  let fixture: ComponentFixture<Bestseller>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Bestseller]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Bestseller);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
