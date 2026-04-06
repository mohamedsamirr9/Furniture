import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-users',
  imports: [CommonModule],
  templateUrl: './users.html',
  styleUrl: './users.css',
})
export class Users {
  users = [
    { name: 'Sarah Miller', email: 'sarah@mail.com', phone: '+1 555-0101', address: '123 Main St, New York', role: 'Customer', joined: '2025-11-01', accountStatus: 'Deleted' },
    { name: 'James Kirk', email: 'james@mail.com', phone: '+1 555-0102', address: '456 Oak Ave, Chicago', role: 'Customer', joined: '2025-12-15', accountStatus: 'Active' },
    { name: 'Nordic Home', email: 'nordic@mail.com', phone: '+1 555-0201', address: '789 Design Blvd, Portland', role: 'Seller', joined: '2025-10-01', accountStatus: 'Active' },
    { name: 'Wood & Co', email: 'wood@mail.com', phone: '+1 555-0203', address: '321 Craft Ln, Denver', role: 'Seller', joined: '2025-09-15', accountStatus: 'Active' },
  ];
}