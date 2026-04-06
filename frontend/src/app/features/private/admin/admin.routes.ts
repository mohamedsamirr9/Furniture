import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Products } from './pages/products/products';
import { Orders } from './pages/orders/orders';
import { Complaints } from './pages/complaints/complaints';
import { Users } from './pages/users/users';

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard},
  { path: 'products', component: Products },
  { path: 'orders', component: Orders},
  { path: 'complaints', component: Complaints },
  { path: 'users', component: Users },
];