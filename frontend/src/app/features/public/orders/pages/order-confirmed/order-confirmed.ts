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
  }

  ngOnInit(): void {}
}