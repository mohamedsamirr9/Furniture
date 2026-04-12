import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, CommonModule, TranslateModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  currentLang: string = 'en';

  constructor(
    private router: Router, 
    public cartService: CartService,
    public wishlistService: WishlistService
  ) {}

  ngOnInit(): void {
    this.currentLang = localStorage.getItem('lang') || 'en';
    this.cartService.loadCart().subscribe();
    this.wishlistService.getWishlist().subscribe();
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
