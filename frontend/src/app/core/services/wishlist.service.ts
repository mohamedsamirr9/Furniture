import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class WishlistService {
  private baseUrl = 'http://localhost:5227/api/favourites';
  
  private wishlistSubject = new BehaviorSubject<any[]>([]);
  public wishlist$ = this.wishlistSubject.asObservable();

  public wishlistCount$: Observable<number> = this.wishlist$.pipe(
    map((items) => items.length)
  );

  constructor(private http: HttpClient) {}

  getWishlist(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl).pipe(
      tap((items) => this.wishlistSubject.next(items))
    );
  }

  addToWishlist(productId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${productId}`, {}).pipe(
      tap((newItem) => {
        const currentItems = this.wishlistSubject.value;
        this.wishlistSubject.next([...currentItems, newItem]);
      })
    );
  }

  removeFromWishlist(productId: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${productId}`).pipe(
      tap(() => {
        const currentItems = this.wishlistSubject.value;
        this.wishlistSubject.next(currentItems.filter(item => item.productId !== productId));
      })
    );
  }
}
