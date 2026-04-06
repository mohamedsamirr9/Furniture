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
    { id: '#1234', customer: 'Sarah M.', seller: 'Nordic Home', total: '$349', status: 'Active' },
    { id: '#1234', customer: 'James K.', seller: 'Comfort Living', total: '$1299', status: 'Active' },
    { id: '#1234', customer: 'Emily R.', seller: 'Wood & Co', total: '$599', status: 'Active' },
    { id: '#1234', customer: 'David L.', seller: 'Simple Form', total: '$199', status: 'Active' },
  ];
}