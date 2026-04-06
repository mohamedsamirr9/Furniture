import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-offers',
  imports: [CommonModule],
  templateUrl: './offers.html',
  styleUrl: './offers.css',
})
export class Offers {
  requests = [
    {
      title: 'Custom Oak Dining Table',
      description: 'Need a 6-seater dining table in solid oak',
      category: 'Tables',
      budget: '$800 - $1000',
      date: '2026-03-15',
      offers: [
        { seller: 'Nordic Home', price: '$850', days: '14 days', status: 'Pending' },
        { seller: 'Nordic Home', price: '$850', days: '14 days', status: 'Pending' },
        { seller: 'Nordic Home', price: '$850', days: '14 days', status: 'Pending' },
      ]
    }
  ];
}