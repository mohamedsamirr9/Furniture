import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-seller-layout',
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './seller-layout.html',
  styleUrl: './seller-layout.css',
})
export class SellerLayout {
  constructor(private router: Router) {}

  signOut() {
    this.router.navigate(['/']);
  }
}