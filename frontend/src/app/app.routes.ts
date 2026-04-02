import { Routes } from '@angular/router';

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
  path: '',
  loadChildren: () =>
    import('./features/private/customer/customer.routes').then((m) => m.CUSTOMER_ROUTES),
},
];
