import { Routes } from '@angular/router';
import { AuthLayout } from '../../../layouts/auth-layout/auth-layout/auth-layout';
import { Login } from './login/login';
import { Register } from './register/register';
import { Role } from './role/role';
import { Verify } from './verify/verify';
import { Success } from './success/success';
import { ForgotPassword } from './forgot-password/forgot-password';
import { customerAuthFlowGuard } from '../../../core/guards/public-role.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    component: AuthLayout,
    canActivate: [customerAuthFlowGuard],
    children: [
      { path: 'signup', redirectTo: 'register', pathMatch: 'full' },
      { path: 'login', component: Login },
      { path: 'register', component: Register },
      { path: 'role', component: Role },
      { path: 'verify', component: Verify },
      { path: 'success', component: Success },
      { path: 'forgot-password', component: ForgotPassword }, 
    ],
  },
];