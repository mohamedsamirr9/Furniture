import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  private readonly baseUrl = `${environment.apiUrl}/search`;

  constructor(private http: HttpClient) {}

  rebuildIndex(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/build-index`, {});
  }
}
