import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-categories',
  imports: [CommonModule],
  templateUrl: './categories.html',
  styleUrl: './categories.css',
})
export class Categories {
categories = [
  { name: 'Sofas', description: 'Living room sofas and sectionals', image: 'sofa.jpg' },
  { name: 'Chairs', description: 'Accent and dining chairs', image: 'chair.jpg' },
  { name: 'Tables', description: 'Dining and coffee tables', image: 'bookshelf.jpg' },
  { name: 'Beds', description: 'Bed frames and headboards', image: 'nightstand.jpg' },
  { name: 'Shelves', description: 'Bookcases and wall shelves', image: 'bookshelf.jpg' },
  { name: 'Lamps', description: 'Floor and pendant lamps', image: 'chair.jpg' },
];
}