import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class WishlistService {
  private baseUrl = `${environment.apiUrl}/favourites`;
  
  private wishlistSubject = new BehaviorSubject<any[]>([]);
  public wishlist$ = this.wishlistSubject.asObservable();

  public wishlistCount$: Observable<number> = this.wishlist$.pipe(
    map((items) => items.length)
  );

  constructor(private http: HttpClient) {}

  getCurrentItems(): any[] {
    return this.wishlistSubject.getValue();
  }

  getWishlist(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl).pipe(
      tap((items) => this.wishlistSubject.next(items))
    );
  }

  addToWishlist(productId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${productId}`, {}).pipe(
      tap((newItem: any) => {
        const currentItems = this.wishlistSubject.value;
        const exists = currentItems.some((item: any) => item.productId === newItem.productId);

        if (!exists) {
          this.wishlistSubject.next([...currentItems, newItem]);
        }
      })
    );
  }

  removeFromWishlist(productId: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${productId}`).pipe(
      tap(() => {
        const currentItems = this.wishlistSubject.value;
        this.wishlistSubject.next(
          currentItems.filter((item: any) => item.productId !== productId)
        );
      })
    );
  }
}
