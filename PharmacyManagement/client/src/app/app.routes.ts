import { Routes } from '@angular/router';
import { PHARMACY_ROUTES } from './pharmacy.routes';

export const routes: Routes = [
  { path: '', children: PHARMACY_ROUTES }
];
