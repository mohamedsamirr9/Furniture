import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    // Redirect to login with return url
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const expectedRoles = route.data['expectedRoles'] as Array<string>;
  if (!expectedRoles || expectedRoles.length === 0) {
    return true;
  }

  const userRole = authService.getUserRole();
  if (userRole && expectedRoles.includes(userRole)) {
    return true;
  }

  console.warn(`Access denied for role: ${userRole}. Expected: ${expectedRoles}`);
  const roleRedirects: Record<string, string> = {
    buyer: '/home',
    seller: '/seller/dashboard',
    admin: '/admin/dashboard',
  };
  const redirectUrl = (userRole && roleRedirects[userRole]) || '/login';
  router.navigate([redirectUrl]);
  return false;
};
