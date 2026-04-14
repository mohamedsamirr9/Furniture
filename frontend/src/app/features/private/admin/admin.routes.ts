import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Products } from './pages/products/products';
import { Orders } from './pages/orders/orders';
import { Complaints } from './pages/complaints/complaints';
import { Users } from './pages/users/users';
import { Categories } from './pages/categories/categories';
import { ShippingRules } from './pages/shipping-rules/shipping-rules';

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard},
  { path: 'categories', component: Categories },
  { path: 'products', component: Products },
  { path: 'orders', component: Orders},
  { path: 'complaints', component: Complaints },
  { path: 'users', component: Users },
  { path: 'shipping-rules', component: ShippingRules },
];