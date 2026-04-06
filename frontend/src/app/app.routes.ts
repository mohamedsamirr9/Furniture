import { Routes } from '@angular/router';
import { PrivateLayoutComponent } from './layouts/private-layout/private-layout';
import { AdminLayout } from './layouts/admin-layout/admin-layout';

export const routes: Routes = [
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
    children: [
      {
        path: '',
        loadChildren: () => import('./features/private/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
      }
    ]
  },
];