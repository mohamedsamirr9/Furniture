import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';
import { ChatWidgetComponent } from '../../shared/components/chat-widget/chat-widget';

@Component({
  selector: 'app-seller-layout',
  imports: [CommonModule, RouterModule, TranslateModule, ChatWidgetComponent],
  templateUrl: './seller-layout.html',
  styleUrl: './seller-layout.css',
})
export class SellerLayout {
  constructor(private router: Router, private authService: AuthService) {}

  signOut() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}