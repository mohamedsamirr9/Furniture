import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-orders',
  imports: [CommonModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders {
  orders = [
    { id: 'ORD-1234', customer: 'Sarah M.', items: 'Nordic Chair ×1', shipping: '123 Main St, New York', status: 'Processing', total: '$349' },
    { id: 'ORD-1235', customer: 'James K.', items: 'Oak Bookshelf ×1', shipping: '456 Oak Ave, Chicago', status: 'Shipped', total: '$1299' },
    { id: 'ORD-1236', customer: 'Emily R.', items: 'Velvet Sofa ×1', shipping: '789 Pine Rd, LA', status: 'Delivered', total: '$599' },
    { id: 'ORD-1237', customer: 'David L.', items: 'Nightstand ×1', shipping: '321 Elm St, Boston', status: 'Cancelled', total: '$199' },
  ];
}