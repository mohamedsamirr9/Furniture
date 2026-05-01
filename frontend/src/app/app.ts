import { Component, signal, Inject, PLATFORM_ID, OnInit } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { WishlistService } from './core/services/wishlist.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class App implements OnInit {
  protected readonly title = signal('Furniture');

  constructor(
    private translate: TranslateService,
    private wishlistService: WishlistService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      const lang = localStorage.getItem('lang') || 'en';

      this.translate.setDefaultLang('en');
      this.translate.use(lang);

      document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';

      const token = localStorage.getItem('token');
      if (token) {
        this.wishlistService.getWishlist().subscribe({
          error: (err) => console.error('Failed to load wishlist on app init', err)
        });
      }
    }
  }
}