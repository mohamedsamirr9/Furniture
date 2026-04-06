import { Routes } from '@angular/router';
import { PublicLayout } from '../../layouts/public-layout/public-layout/public-layout';
import { Home } from './home/pages/home/home';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { ProductsList } from './products/pages/products-list/products-list';
import { ProductDetails } from './products/pages/product-details/product-details';

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    component: PublicLayout,
    children: [
      { path: '', component: Home },
      { path: 'products', component: ProductsList },
      { path: 'categories/:id/products', component: ProductsList },
      { path: 'products/:id', component: ProductDetails },
    ],
  },
];
