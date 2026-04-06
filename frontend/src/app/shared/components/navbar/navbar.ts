import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  constructor(private router: Router, public cartService: CartService) {}

  ngOnInit(): void {
    this.cartService.loadCart().subscribe();
  }

  get isDarkPage(): boolean {
    return this.router.url !== '/';
  }
}
