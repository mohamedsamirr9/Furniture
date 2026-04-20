import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ShippingRule, ShippingRuleCreateUpdate } from '../models/shipping-rule.model';

@Injectable({
  providedIn: 'root'
})
export class ShippingRulesService {
    private apiUrl = `${environment.apiUrl}/ShippingRules`;


  constructor(private http: HttpClient) {}

  getAll(city?: string, categoryId?: number): Observable<ShippingRule[]> {
    let params = new HttpParams();
    if (city) params = params.set('city', city);
    if (categoryId) params = params.set('categoryId', categoryId.toString());

    return this.http.get<ShippingRule[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<ShippingRule> {
    return this.http.get<ShippingRule>(`${this.apiUrl}/${id}`);
  }

  create(rule: ShippingRuleCreateUpdate): Observable<ShippingRule> {
    return this.http.post<ShippingRule>(this.apiUrl, rule);
  }

  update(id: number, rule: ShippingRuleCreateUpdate): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, rule);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
