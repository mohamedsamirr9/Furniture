import { Component } from '@angular/core';

@Component({
  selector: 'app-product-details',
  imports: [],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails {
  product = {
    id: 2,
    name: 'Modern Scandinavian Salon',
    workshopName: 'Abu El Karam Workshop',
    description: 'Luxurious sage green velvet sofa with clean lines and superior comfort.',
    price: 459,
    image:
      'https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?q=80&w=800&auto=format&fit=crop',
  };
}
