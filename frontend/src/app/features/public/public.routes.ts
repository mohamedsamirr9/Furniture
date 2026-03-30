import { Routes } from '@angular/router';
import { PublicLayout } from '../../layouts/public-layout/public-layout/public-layout';
import { Home } from './home/pages/home/home';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    component: PublicLayout,
    children: [{ path: '', component: Home }],
  },
];
