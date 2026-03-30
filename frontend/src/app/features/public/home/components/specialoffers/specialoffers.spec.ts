import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Specialoffers } from './specialoffers';

describe('Specialoffers', () => {
  let component: Specialoffers;
  let fixture: ComponentFixture<Specialoffers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Specialoffers]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Specialoffers);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
