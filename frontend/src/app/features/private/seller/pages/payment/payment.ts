import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment {

  // ── Summary Cards ──
  earnings = {
    total: 0,
    pending: 0,
    lastPayout: 0,
  };

  // ── Bank Details Form ──
  bankForm = {
    bankName: '',
    accountHolder: '',
    iban: '',
    swift: '',
    payoutCycle: '',
    verified: false,
  };

  saveBankDetails() {
  
    console.log('Bank details saved:', this.bankForm);
    this.bankForm.verified = true;
  }

  recentPayouts: { id: string; date: string; amount: number; status: string }[] = [];

  newPayout = { id: '', date: '', amount: 0 };

  addPayout() {
    if (!this.newPayout.id || !this.newPayout.date || !this.newPayout.amount) return;
    this.recentPayouts.push({ ...this.newPayout, status: 'PAID' });
    this.earnings.lastPayout = this.newPayout.amount;
    this.newPayout = { id: '', date: '', amount: 0 };
  }

  payments = [
    { id: 'pay-1', order: 'ORD-1234', reference: 'TXN-98765', status: 'Completed', date: '2026-03-18' },
    { id: 'pay-2', order: 'ORD-1234', reference: 'TXN-98765', status: 'Completed', date: '2026-03-15' },
   
  ];
}