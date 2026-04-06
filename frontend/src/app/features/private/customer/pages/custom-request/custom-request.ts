import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-custom-request',
  imports: [CommonModule, FormsModule],
templateUrl: './custom-request.html',
 styleUrl: './custom-request.css',
})
export class CustomRequestComponent {
  formData = {
    fullName: '',
    email: '',
    furnitureType: '',
    dimensions: '',
    material: '',
    budgetRange: '',
    description: '',
  };

  myRequests = [
    {
      id: 1,
      name: 'Custom Oak Dining Table',
      description: 'Need a 6-seater dining table in solid oak',
      status: 'Open',
    },
    {
      id: 2,
      name: 'Corner Bookshelf Unit',
      description: 'L-shaped bookshelf for living room corner',
      status: 'In Progress',
    },
  ];

  constructor(private router: Router) {}

  onSubmit() {
    this.router.navigate(['/customer/success']);
  }

  viewOffer(requestId: number) {
    this.router.navigate(['/customer/compare-offers', requestId]);
  }
}