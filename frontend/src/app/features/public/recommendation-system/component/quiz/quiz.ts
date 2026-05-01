import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RecommendationService } from '../../../../../core/services/recommendation.service';
import { QuizDto } from '../../../../../core/models/recommendation.model';

interface Step {
  key: keyof QuizDto;
  question: string;
  options: { label: string; icon: string }[];
}

@Component({
  selector: 'app-quiz',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quiz.html',
  styleUrls: ['./quiz.css']    
})
export class QuizComponent {
 steps: Step[] = [
  {
    key: 'style',
    question: 'What\'s your furniture style?',
    options: [
      { label: 'Modern',     icon: 'bi bi-grid-3x3-gap' },
      { label: 'Classic',    icon: 'bi bi-building' },
      { label: 'Minimalist', icon: 'bi bi-circle' },
      { label: 'Rustic',     icon: 'bi bi-tree' }
    ]
  },
  {
    key: 'color',
    question: 'Preferred color palette?',
    options: [
      { label: 'Neutral',   icon: 'bi bi-circle-half' },
      { label: 'Dark',      icon: 'bi bi-moon-fill' },
      { label: 'Light',     icon: 'bi bi-sun' },
      { label: 'Colorful',  icon: 'bi bi-palette' }
    ]
  },
  {
    key: 'roomSize',
    question: 'How big is your room?',
    options: [
      { label: 'Small',     icon: 'bi bi-box' },
      { label: 'Medium',    icon: 'bi bi-house' },
      { label: 'Large',     icon: 'bi bi-house-door' },
      { label: 'Open plan', icon: 'bi bi-arrows-fullscreen' }
    ]
  },
  {
    key: 'budget',
    question: 'What\'s your budget range?',
    options: [
      { label: 'Budget',    icon: 'bi bi-wallet2' },
      { label: 'Mid-range', icon: 'bi bi-credit-card' },
      { label: 'Premium',   icon: 'bi bi-gem' },
      { label: 'Luxury',    icon: 'bi bi-trophy' }
    ]
  }
];

  currentStep = 0;
  answers: Partial<QuizDto> = {};
  loading = false;
  error = '';

  get progress(): number {
    return ((this.currentStep + 1) / this.steps.length) * 100;
  }

  get currentAnswer(): string {
    return this.answers[this.steps[this.currentStep].key] ?? '';
  }

  constructor(
    private service: RecommendationService,
    private router: Router
  ) {}

  select(label: string): void {
    this.answers[this.steps[this.currentStep].key] = label;
  }

  back(): void {
    if (this.currentStep > 0) this.currentStep--;
  }

  next(): void {
    if (!this.currentAnswer) return;
    if (this.currentStep < this.steps.length - 1) {
      this.currentStep++;
    } else {
      this.submit();
    }
  }

  submit(): void {
  this.loading = true;
  this.error = '';
  this.service.saveQuiz(this.answers as QuizDto).subscribe({
    next: () => this.router.navigate(['/']),
    error: () => {
      this.loading = false;
      this.error = 'Something went wrong. Please try again.';
    }
  });
  }
}