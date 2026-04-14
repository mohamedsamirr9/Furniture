import { Routes } from '@angular/router';
import { CustomRequestComponent } from './pages/custom-request/custom-request';
import { CompareOffers } from './pages/compare-offers/compare-offers';
import { SubmitSuccess } from './pages/submit-success/submit-success';
import { Complaints } from './pages/complaints/complaints';
import { NewComplaint } from './pages/new-complaint/new-complaint';


export const CUSTOMER_ROUTES: Routes = [
  { path: '', component: CustomRequestComponent },
  { path: 'compare-offers/:id', component: CompareOffers },
  { path: 'success', component: SubmitSuccess },
 { path: 'complaints', component: Complaints },
  { path: 'new-complaint', component: NewComplaint },
  
];
