import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product',
  imports: [CommonModule],
  templateUrl: './product.html',
  styleUrl: './product.css',
})
export class Product {
products = [
  { name: 'Nordic Accent Chair', stock: '15 units', price: '$349', status: 'Available', image: 'chair.jpg' },
  { name: 'Sage Velvet Sofa', stock: '8 units', price: '$1299', status: 'Available', image: 'sofa.jpg' },
  { name: 'Oak Display Bookshelf', stock: '12 units', price: '$599', status: 'Available', image: 'bookshelf.jpg' },
  { name: 'Minimalist Nightstand', stock: '25 units', price: '$199', status: 'Available', image: 'nightstand.jpg' },
];
}