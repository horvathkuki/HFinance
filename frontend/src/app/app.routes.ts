import { Routes } from '@angular/router';
import { LoginPage } from './pages/login.page';
import { RegisterPage } from './pages/register.page';
import { DashboardPage } from './pages/dashboard.page';
import { AccountPage } from './pages/account.page';
import { AdminUsersPage } from './pages/admin-users.page';
import { HoldingsPage } from './pages/holdings.page';
import { AllocationsPage } from './pages/allocations.page';
import { HistoryPage } from './pages/history.page';
import { SnapshotsPage } from './pages/snapshots.page';
import { adminGuard, authGuard } from './auth.guards';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginPage },
  { path: 'register', component: RegisterPage },
  { path: 'dashboard', component: DashboardPage, canActivate: [authGuard] },
  { path: 'holdings', component: HoldingsPage, canActivate: [authGuard] },
  { path: 'allocations', component: AllocationsPage, canActivate: [authGuard] },
  { path: 'history', component: HistoryPage, canActivate: [authGuard] },
  { path: 'snapshots', component: SnapshotsPage, canActivate: [authGuard] },
  { path: 'settings', component: AccountPage, canActivate: [authGuard] },
  { path: 'account', pathMatch: 'full', redirectTo: 'settings' },
  { path: 'admin/users', component: AdminUsersPage, canActivate: [authGuard, adminGuard] },
  { path: '**', redirectTo: 'login' },
];
