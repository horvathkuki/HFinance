import { Routes } from '@angular/router';
import { LoginPage } from './pages/login.page';
import { RegisterPage } from './pages/register.page';
import { DashboardPage } from './pages/dashboard.page';
import { AccountPage } from './pages/account.page';
import { AdminUsersPage } from './pages/admin-users.page';
import { adminGuard, authGuard } from './auth.guards';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', component: LoginPage },
  { path: 'register', component: RegisterPage },
  { path: 'dashboard', component: DashboardPage, canActivate: [authGuard] },
  { path: 'account', component: AccountPage, canActivate: [authGuard] },
  { path: 'admin/users', component: AdminUsersPage, canActivate: [authGuard, adminGuard] },
  { path: '**', redirectTo: 'dashboard' },
];
