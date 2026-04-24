import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { Cart, CartItem } from '../models/cart.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private baseUrl = `${environment.apiUrl}/cart`;
  
  private cartSubject = new BehaviorSubject<Cart | null>(null);
  public cart$ = this.cartSubject.asObservable();

  public cartCount$: Observable<number> = this.cart$.pipe(
    map((cart) => (cart && cart.items ? cart.items.reduce((total, item) => total + item.quantity, 0) : 0))
  );

  constructor(private http: HttpClient) {}

  loadCart(): Observable<Cart> {
    return this.http.get<Cart>(this.baseUrl).pipe(
      tap((cart) => this.cartSubject.next(cart))
    );
  }

  addToCart(productId: number, quantity: number): Observable<Cart> {
    const payload = { productId, quantity };
    return this.http.post<Cart>(`${this.baseUrl}/items`, payload).pipe(
      tap((updatedCart) => this.cartSubject.next(updatedCart))
    );
  }

  updateQuantity(productId: number, quantity: number): Observable<Cart> {
    const payload = { quantity };
    return this.http.put<Cart>(`${this.baseUrl}/items/${productId}`, payload).pipe(
      tap((updatedCart) => this.cartSubject.next(updatedCart))
    );
  }

  removeCartItem(productId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.baseUrl}/items/${productId}`).pipe(
      tap((updatedCart) => this.cartSubject.next(updatedCart))
    );
  }

  clearCart(): Observable<any> {
    return this.http.delete(`${this.baseUrl}`).pipe(
      tap(() => this.cartSubject.next({ items: [], totalPrice: 0 }))
    );
  }
}
