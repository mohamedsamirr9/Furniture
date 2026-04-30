import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomRequestDetailsComponent } from './custom-request-details';

describe('CustomRequestDetailsComponent', () => {
  let component: CustomRequestDetailsComponent;
  let fixture: ComponentFixture<CustomRequestDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomRequestDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomRequestDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
