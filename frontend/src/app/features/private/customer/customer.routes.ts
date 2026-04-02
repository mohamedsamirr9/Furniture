import { Routes } from '@angular/router';
import { CustomerLayout } from '../../../layouts/customer-layout/customer-layout/customer-layout';
export const CUSTOMER_ROUTES: Routes = [
  {
    path: 'customer',
    component: CustomerLayout,
    children: [
      { path: '', redirectTo: 'my-requests', pathMatch: 'full' },
      {
        path: 'my-requests',
        loadComponent: () =>
          import('./my-requests/pages/my-requests/my-requests').then((m) => m.MyRequests),
      },
      {
        path: 'custom-request',
        loadComponent: () =>
          import('./custom-request/pages/custom-request/custom-request').then((m) => m.CustomRequest),
      },
      {
        path: 'compare-offer',
        loadComponent: () =>
          import('./compare-offer/pages/compare-offer/compare-offer').then((m) => m.CompareOffer),
      },
    ],
  },
];