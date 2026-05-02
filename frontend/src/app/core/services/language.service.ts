import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  constructor(@Inject(PLATFORM_ID) private platformId: object) {}

  getCurrentLang(): string {
    if (!isPlatformBrowser(this.platformId)) {
      return 'en';
    }
    return localStorage.getItem('lang') || 'en';
  }

  /** Same behavior as the customer navbar: flip EN ↔ AR, persist, full reload. */
  toggleLanguage(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    const current = this.getCurrentLang();
    const next = current === 'en' ? 'ar' : 'en';
    localStorage.setItem('lang', next);
    window.location.reload();
  }
}
