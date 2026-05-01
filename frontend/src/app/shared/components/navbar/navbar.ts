import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { ChatWidgetComponent } from '../chat-widget/chat-widget';
import { NotificationComponent } from '../../../features/public/notification/component/notification/notification';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule, TranslateModule, ChatWidgetComponent, NotificationComponent],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css'],
})
export class Navbar implements OnInit, OnDestroy {
  currentLang: string = 'en';
  dropdownOpen = false;

  private outsideClickListener = (e: MouseEvent) => {
    const wrapper = document.querySelector('.profile-dropdown-wrapper');
    if (wrapper && !wrapper.contains(e.target as Node)) {
      this.dropdownOpen = false;
    }
  };

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
    }

    document.addEventListener('click', this.outsideClickListener);
  }

  ngOnDestroy(): void {
    document.removeEventListener('click', this.outsideClickListener);
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  closeDropdown(): void {
    this.dropdownOpen = false;
  }

  getUserInitial(user: any): string {
    return user?.name?.charAt(0)?.toUpperCase() ||
           user?.email?.charAt(0)?.toUpperCase() || 'U';
  }

  logout(): void {
    this.closeDropdown();
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