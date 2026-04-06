import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  constructor(private router: Router) {}

  get isDarkPage(): boolean {
    return this.router.url.includes('products') || this.router.url.includes('categories');
  }
}
