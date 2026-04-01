import { Component } from '@angular/core';

@Component({
  selector: 'app-products-list',
  imports: [],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css',
})
export class ProductsList {
  products = [
    {
      id: 1,
      name: 'Modern Dining Table',
      price: 599,
      rating: 4.8,
      image: 'https://images.unsplash.com/photo-1657524398377-567034729507?q=80&w=500',
    },
    {
      id: 2,
      name: 'Cozy Gray Salon',
      price: 459,
      rating: 4.9,
      image: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?q=80&w=500',
    },
    {
      id: 3,
      name: 'Minimalist Chairs',
      price: 699,
      rating: 4.8,
      image: 'https://images.unsplash.com/photo-1592078615290-033ee584e267?q=80&w=500',
    },
    {
      id: 4,
      name: 'Elegant Lighting',
      price: 120,
      rating: 4.7,
      image: 'https://images.unsplash.com/photo-1576514409676-dbcb2640bf58?q=80&w=500',
    },
    {
      id: 5,
      name: 'Kitchen Cabinet',
      price: 850,
      rating: 4.6,
      image: 'https://images.unsplash.com/photo-1591924265219-1ea350ab7279?q=80&w=500',
    },
    {
      id: 6,
      name: 'Luxury Bathroom Set',
      price: 340,
      rating: 4.5,
      image: 'https://images.unsplash.com/photo-1584622650111-993a426fbf0a?q=80&w=500',
    },
  ];
}
