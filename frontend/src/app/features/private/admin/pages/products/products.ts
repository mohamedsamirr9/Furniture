import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-products',
  imports: [CommonModule],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products {
  products = [
    { name: 'Nordic Accent Chair', seller: 'Nordic Home', price: '$349', status: 'Active' },
    { name: 'Sage Velvet Sofa', seller: 'Comfort Living', price: '$1299', status: 'Active' },
    { name: 'Oak Display Bookshelf', seller: 'Wood & Co', price: '$599', status: 'Active' },
    { name: 'Minimalist Nightstand', seller: 'Simple Form', price: '$199', status: 'Active' },
    { name: 'Brass Pendant Lamp', seller: 'Lux Lights', price: '$129', status: 'Active' },
  ];
}