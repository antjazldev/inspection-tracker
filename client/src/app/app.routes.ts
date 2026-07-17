import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'inspections' },
  {
    path: 'inspections',
    loadComponent: () =>
      import('./features/inspections/inspection-list.component').then(
        (m) => m.InspectionListComponent
      ),
  },
  {
    path: 'inspections/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/inspections/inspection-form.component').then(
        (m) => m.InspectionFormComponent
      ),
  },
  {
    path: 'inspections/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/inspections/inspection-form.component').then(
        (m) => m.InspectionFormComponent
      ),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register.component').then((m) => m.RegisterComponent),
  },
  { path: '**', redirectTo: 'inspections' },
];
