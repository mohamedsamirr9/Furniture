import { Component, OnInit } from '@angular/core';
import { Aboutus } from '../../components/aboutus/aboutus';
import { Contactus } from '../../components/contactus/contactus';
import { Bestseller } from '../../components/bestseller/bestseller';
import { Specialoffers } from '../../components/specialoffers/specialoffers';
import { Hero } from '../../components/hero/hero';
import { Categories } from '../../components/categories/categories';
import { CustomOrder } from '../../components/custom-order/custom-order';
import { HomeRecommendationsComponent } from '../../components/home-recommendations/home-recommendations';
import { QuizPromptComponent } from '../../components/quiz-prompt/quiz-prompt';
import { RecommendationService } from '../../../../../core/services/recommendation.service';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../../../core/services/auth.service';

@Component({
  selector: 'app-home',
  imports: [
    Aboutus,
    Contactus,
    Bestseller,
    Specialoffers,
    Hero,
    Categories,
    CustomOrder,
    HomeRecommendationsComponent,
    QuizPromptComponent,
    NgIf
  ],
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
})
export class Home implements OnInit {
  isLoggedIn = false;
  isBuyer = false;
  quizCompleted = false;
  checked = false;

  constructor(
    private recService: RecommendationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');

    if (!token) {
      this.checked = true;
      return;
    }

    this.isLoggedIn = true;
    this.isBuyer = this.authService.getUserRole() === 'buyer';

    if (!this.isBuyer) {
      this.checked = true;
      return;
    }

    this.recService.getQuizStatus().subscribe({
      next: (res) => {
        this.quizCompleted = res.isCompleted;
        this.checked = true;
      },
      error: () => {
        this.checked = true;
      }
    });
  }
}