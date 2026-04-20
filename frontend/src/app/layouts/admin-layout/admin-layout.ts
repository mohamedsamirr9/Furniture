import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout {
  menuItems = [
    { label: 'Dashboard', path: '/admin/dashboard', icon: '⊞' },
    { label: 'Products', path: '/admin/products', icon: '' },
    { label: 'Orders', path: '/admin/orders', icon: '' },
    { label: 'Complaints', path: '/admin/complaints', icon: '' },
    { label: 'Users', path: '/admin/users', icon: '' },
  ];

  constructor(private router: Router, private authService: AuthService) {}

  navigate(path: string) {
    this.router.navigate([path]);
  }

  signOut() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}