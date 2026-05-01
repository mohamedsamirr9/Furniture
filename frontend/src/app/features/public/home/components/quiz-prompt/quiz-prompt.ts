import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-quiz-prompt',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './quiz-prompt.html',
  styleUrls: ['./quiz-prompt.css']
})
export class QuizPromptComponent implements OnInit {
  isOpen = false;
  isDismissed = false;

  ngOnInit(): void {
    setTimeout(() => {
      this.isOpen = true;
    }, 2000);
  }

  dismiss(): void {
    this.isOpen = false;
    this.isDismissed = true;
  }

  reopen(): void {
    this.isOpen = true;
  }
}