// import { Component, OnInit, ViewEncapsulation } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { Router, RouterModule } from '@angular/router';

// import { TranslateModule } from '@ngx-translate/core';

// @Component({
//   selector: 'app-order-confirmed',
//   standalone: true,
//   imports: [CommonModule, RouterModule, TranslateModule],
//   templateUrl: './order-confirmed.html',
//   styleUrls: ['./order-confirmed.css'],
//    encapsulation: ViewEncapsulation.None
// })
// export class OrderConfirmedComponent implements OnInit {
//   orderResponse: any;

//   constructor(private router: Router) {
//     const nav = this.router.getCurrentNavigation();
//     if (nav?.extras.state && nav.extras.state['orderResponse']) {
//       this.orderResponse = nav.extras.state['orderResponse'];
//     }
//   }

//   ngOnInit(): void {
//   }
// }



import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-order-confirmed',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './order-confirmed.html',
  styleUrls: ['./order-confirmed.css'],
  encapsulation: ViewEncapsulation.None
})
export class OrderConfirmedComponent implements OnInit {
  orderResponse: any;
  orders: any[] = [];
  totalProcessed = 0;
  primaryOrderId: number | null = null;

  constructor(private router: Router) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state;
    if (state && state['orderResponse']) {
      this.orderResponse = state['orderResponse'];
    }
    // fallback: لو جه من history state (بعض المتصفحات)
    const historyState = history.state;
    if (!this.orderResponse && historyState?.orderResponse) {
      this.orderResponse = historyState.orderResponse;
    }

    this.hydrateFromResponse(this.orderResponse);
  }

  ngOnInit(): void {}

  private hydrateFromResponse(response: any): void {
    if (!response) return;

    const orders = response?.orders ?? response?.Orders;
    if (Array.isArray(orders) && orders.length > 0) {
      this.orders = orders;
      this.primaryOrderId = orders[0]?.orderId ?? orders[0]?.OrderId ?? orders[0]?.id ?? null;
      this.totalProcessed = orders.reduce((sum: number, o: any) => sum + (Number(o?.totalPrice ?? o?.TotalPrice ?? 0) || 0), 0);
      return;
    }

    // legacy single-order response
    this.orders = [response];
    this.primaryOrderId = response?.orderId ?? response?.OrderId ?? response?.id ?? null;
    this.totalProcessed = Number(response?.totalPrice ?? response?.TotalPrice ?? 0) || 0;
  }
}