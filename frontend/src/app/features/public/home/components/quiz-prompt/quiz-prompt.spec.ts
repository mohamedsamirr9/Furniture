import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuizPrompt } from './quiz-prompt';

describe('QuizPrompt', () => {
  let component: QuizPrompt;
  let fixture: ComponentFixture<QuizPrompt>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuizPrompt]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QuizPrompt);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
