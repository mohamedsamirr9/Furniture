import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  QuizDto,
  ProductRecommendationDto,
  ActionDto,
  QuizStatusDto
} from '../models/recommendation.model';

@Injectable({ providedIn: 'root' })
export class RecommendationService {
  private base = `${environment.apiUrl}/recommendations`;

  constructor(private http: HttpClient) {}

  getQuizStatus(): Observable<QuizStatusDto> {
    return this.http.get<QuizStatusDto>(`${this.base}/quiz-status`);
  }

  saveQuiz(dto: QuizDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/quiz`, dto);
  }

  getRecommendations(topK = 10): Observable<ProductRecommendationDto[]> {
    return this.http.get<ProductRecommendationDto[]>(this.base, {
      params: { topK }
    });
  }

  trackAction(dto: ActionDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/action`, dto);
  }

  trackClick(productId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/action`, {
      productId,
      actionType: 'click'
    });
  }
}