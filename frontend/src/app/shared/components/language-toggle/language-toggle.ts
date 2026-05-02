import { Component, OnInit } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-language-toggle',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './language-toggle.html',
  styleUrl: './language-toggle.css',
})
export class LanguageToggleComponent implements OnInit {
  /** Label for the language you will switch *to* (matches customer navbar). */
  targetLangLabel: 'EN' | 'AR' = 'AR';

  constructor(private languageService: LanguageService) {}

  ngOnInit(): void {
    this.refreshLabel();
  }

  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }

  private refreshLabel(): void {
    this.targetLangLabel = this.languageService.getCurrentLang() === 'en' ? 'AR' : 'EN';
  }
}
