import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminUsersService } from '../admin-users.service';
import { UserProfile } from '../models';

@Component({
  selector: 'app-admin-users-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page">
      <h1>Admin - Users</h1>
      <button (click)="load()">Refresh</button>
      <table *ngIf="users().length > 0">
        <thead>
          <tr>
            <th>Email</th>
            <th>Name</th>
            <th>Roles</th>
            <th>Status</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let user of users()">
            <td>{{ user.email }}</td>
            <td>{{ user.displayName }}</td>
            <td>{{ user.roles.join(', ') }}</td>
            <td>{{ user.isActive ? 'Active' : 'Inactive' }}</td>
            <td>
              <button (click)="toggleStatus(user)">{{ user.isActive ? 'Disable' : 'Enable' }}</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  `,
  styles: `
    .page { padding: 1rem; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #d0d0d0; padding: 0.5rem; text-align: left; }
  `,
})
export class AdminUsersPage {
  readonly users = signal<UserProfile[]>([]);

  constructor(private readonly adminUsersService: AdminUsersService) {
    this.load();
  }

  load(): void {
    this.adminUsersService.getUsers().subscribe({
      next: (response) => this.users.set(response.items),
    });
  }

  toggleStatus(user: UserProfile): void {
    this.adminUsersService.setStatus(user.id, !user.isActive).subscribe({
      next: () => this.load(),
    });
  }
}
