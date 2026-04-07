import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';


@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './wishlist.html',
  styleUrl: './wishlist.css',


})
export class WishlistComponent implements OnInit {

  loading = true;

  wishlistItems = [
    {
      id: 1,
      name: 'Modern Chair',
      price: 500,
      originalPrice: 650,
      rating: 4,
      reviewCount: 12,
      mainImage: 'assets/image1.jpg',
      brand: 'IKEA'
    },
    {
      id: 2,
      name: 'Wooden Table',
      price: 1200,
      rating: 5,
      reviewCount: 30,
      mainImage: 'assets/image2.jpg',
      brand: 'Home Center'
    },
    {
      id: 3,
      name: 'Luxury Sofa',
      price: 3000,
      rating: 5,
      reviewCount: 55,
      mainImage: 'assets/image3.jpg',
      brand: 'Furnora'
    }
  ];

  ngOnInit() {
    setTimeout(() => {
      this.loading = false;
    }, 1000);
  }
viewProduct(id: number) {

  alert('Go to product details for ID: ' + id);
}
  removeItem(id: number) {
    this.wishlistItems = this.wishlistItems.filter(item => item.id !== id);
  }

  addToCart(product: any) {
    alert(product.name + ' added to cart 🛒');
  }

  getStars(): number[] {
    return [1, 2, 3, 4, 5];
  }
}