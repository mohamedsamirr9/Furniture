import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

const ADMIN_ROOT = '/admin';
const SELLER_ROOT = '/seller';
const BUYER_HOME = '/';

function homeForRole(role: string | null): string[] {
  switch (role) {
    case 'admin':
      return [ADMIN_ROOT];
    case 'seller':
      return [SELLER_ROOT];
    case 'buyer':
      return [BUYER_HOME];
    default:
      return [BUYER_HOME];
  }
}

/**
 * Marketing / customer home (`/`). Guests OK; logged-in admin/seller → their dashboard.
 * Buyers stay on home.
 */
export const marketplaceHomeGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return true;
  }

  const role = auth.getUserRole();
  if (role === 'admin' || role === 'seller') {
    router.navigate(homeForRole(role));
    return false;
  }

  return true;
};

/**
 * Login, register, and other auth-layout pages: for guests only.
 * Any authenticated user is sent to the correct home for their role.
 */
export const customerAuthFlowGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return true;
  }

  router.navigate(homeForRole(auth.getUserRole()));
  return false;
};
