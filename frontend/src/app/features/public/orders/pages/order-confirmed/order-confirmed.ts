import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-order-confirmed',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './order-confirmed.html',
  styleUrls: ['./order-confirmed.css']
})
export class OrderConfirmedComponent implements OnInit {
  orderResponse: any;

  constructor(private router: Router) {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state && nav.extras.state['orderResponse']) {
      this.orderResponse = nav.extras.state['orderResponse'];
    }
  }

  ngOnInit(): void {
  }
}
