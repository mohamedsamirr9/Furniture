import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Order, CreateOrder, CreateOrderFromOffer } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = 'http://localhost:5227/api/orders';

  constructor(private http: HttpClient) {}

  createOrder(data: CreateOrder): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  createOrderFromOffer(data: CreateOrderFromOffer): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/from-offer`, data);
  }

  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  getOrderById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }

  cancelOrder(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getAllOrdersPaginated(pageIndex: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/admin/all?pageIndex=${pageIndex}&pageSize=${pageSize}`);
  }

  getOrdersByStatus(status: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/admin/status/${status}`);
  }

  updateOrderStatus(orderId: number, status: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/admin/${orderId}/status`, { status });
  }
}
