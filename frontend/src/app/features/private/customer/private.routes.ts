import { Routes } from '@angular/router';
import { CustomRequestComponent } from './pages/custom-request/custom-request';
import { CompareOffers } from './pages/compare-offers/compare-offers';
import { SubmitSuccess } from './pages/submit-success/submit-success';

export const CUSTOMER_ROUTES: Routes = [
  { path: '', component: CustomRequestComponent },
  { path: 'compare-offers/:id', component: CompareOffers },
  { path: 'success', component: SubmitSuccess },
];