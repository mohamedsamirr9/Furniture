import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { ChatWidgetComponent } from '../chat-widget/chat-widget';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule, TranslateModule, ChatWidgetComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  currentLang: string = 'en';

  constructor(
    private router: Router, 
    public cartService: CartService,
    public wishlistService: WishlistService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentLang = localStorage.getItem('lang') || 'en';
    if (this.authService.isLoggedIn()) {
      this.cartService.loadCart().subscribe();
      this.wishlistService.getWishlist().subscribe();
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  get isDarkPage(): boolean {
    return this.router.url !== '/';
  }

  toggleLanguage(): void {
    this.currentLang = this.currentLang === 'en' ? 'ar' : 'en';
    localStorage.setItem('lang', this.currentLang);
    window.location.reload();
  }
}
