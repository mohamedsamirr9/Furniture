import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-compare-offers',
  imports: [CommonModule],
  templateUrl: './compare-offers.html',
  styleUrl: './compare-offers.css',
})
export class CompareOffers {
  offers = [
    { name: 'Nordic Home', price: '$850', delivery: '2 weeks', material: 'Oak Wood', rating: 4.8 },
    { name: 'Comfort Living', price: '$850', delivery: '2 weeks', material: 'Oak Wood', rating: 4.8 },
    { name: 'Karam Home', price: '$850', delivery: '2 weeks', material: 'Oak Wood', rating: 4.8 },
  ];

  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/customer']);
  }

acceptOffer(offer: any) {
  // alert('Offer Accepted: ' + offer.name);
}
}