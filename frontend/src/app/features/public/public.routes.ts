import { Routes } from '@angular/router';
import { PublicLayout } from '../../layouts/public-layout/public-layout/public-layout';
import { Home } from './home/pages/home/home';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { ProductsList } from './products/pages/products-list/products-list';
import { ProductDetails } from './products/pages/product-details/product-details';
import { CartComponent } from './cart/pages/cart/cart';
import { CheckoutComponent } from './orders/pages/checkout/checkout';
import { OrderConfirmedComponent } from './orders/pages/order-confirmed/order-confirmed';
import { MyOrdersComponent } from './orders/pages/my-orders/my-orders';
import { OrderDetailsComponent } from './orders/pages/order-details/order-details';
import { WishlistComponent } from './wishlist/wishlist/wishlist';

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    component: PublicLayout,
    children: [
      { path: '', component: Home },
      { path: 'products', component: ProductsList },
      { path: 'products/:id', component: ProductDetails },
      { path: 'cart', component: CartComponent },
      { path: 'checkout', component: CheckoutComponent },
      { path: 'orders/confirmed', component: OrderConfirmedComponent },
      { path: 'orders/:id', component: OrderDetailsComponent },
      { path: 'orders', component: MyOrdersComponent },
      { path: 'wishlist', component: WishlistComponent },
    ],
  },
];
