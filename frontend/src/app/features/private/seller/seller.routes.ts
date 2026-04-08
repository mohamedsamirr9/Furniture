import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Categories } from '../admin/pages/categories/categories';
import { Product } from './pages/product/product';
import { Orders } from './pages/orders/orders';
import { Offers } from './pages/offers/offers';
import { Payment } from './pages/payment/payment';
import { Complaints } from './pages/complaints/complaints';

export const SELLER_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'product', component: Product },
  { path: 'orders', component: Orders },
  { path: 'offers', component: Offers },
  { path: 'payment', component: Payment },
  { path: 'complaints', component: Complaints },
];