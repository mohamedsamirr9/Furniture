import { Routes } from '@angular/router';
import { AuthLayout } from '../../../layouts/auth-layout/auth-layout/auth-layout';
import { Login } from './login/login';
import { Register } from './register/register';
import { Role } from './role/role';
import { Verify } from './verify/verify';
import { Success } from './success/success';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    component: AuthLayout,
    children: [
      { path: 'login', component: Login },
      { path: 'register', component: Register },
      { path: 'role', component: Role },
      { path: 'verify', component: Verify },
      { path: 'success', component: Success },
    ],
  },
];
