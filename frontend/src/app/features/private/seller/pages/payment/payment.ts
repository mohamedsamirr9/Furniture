import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment',
  imports: [CommonModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment {
  payments = [
    { id: 'pay-1', order: 'ORD-1234', reference: 'TXN-98765', status: 'Completed', date: '2026-03-18' },
    { id: 'pay-2', order: 'ORD-1234', reference: 'TXN-98765', status: 'Completed', date: '2026-03-15' },
    { id: 'pay-3', order: 'ORD-1234', reference: 'TXN-98765', status: 'Completed', date: '2026-03-10' },
    { id: 'pay-4', order: 'ORD-1234', reference: 'TXN-98765', status: 'Refunded', date: '2026-03-08' },
  ];
}