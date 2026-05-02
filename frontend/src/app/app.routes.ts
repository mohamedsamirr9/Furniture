import { Routes } from '@angular/router';
import { PrivateLayoutComponent } from './layouts/private-layout/private-layout';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { SellerLayout } from './layouts/seller-layout/seller-layout';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'home', redirectTo: '', pathMatch: 'full' },
  {
    path: '',
    loadChildren: () => import('./features/public/public.routes').then((m) => m.PUBLIC_ROUTES),
  },
  {
    path: '',
    loadChildren: () => import('./features/public/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'customer',
    component: PrivateLayoutComponent,
    canActivate: [authGuard],
    data: { expectedRoles: ['buyer'] },
    children: [
      {
        path: '',
        loadChildren: () => import('./features/private/customer/private.routes').then((m) => m.CUSTOMER_ROUTES),
      }
    ]
  },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [authGuard],
    data: { expectedRoles: ['admin'] },
    children: [
      {
        path: '',
        loadChildren: () => import('./features/private/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
      }
    ]
  },
  {
    path: 'seller',
    component: SellerLayout,
    canActivate: [authGuard],
    data: { expectedRoles: ['seller'] },
    children: [
      {
        path: '',
        loadChildren: () => import('./features/private/seller/seller.routes').then((m) => m.SELLER_ROUTES),
      }
    ]
  },
];