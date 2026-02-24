import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminUsersService } from '../admin-users.service';
import { UserProfile } from '../models';

@Component({
  selector: 'app-admin-users-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="hf-page">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h1 class="h3 mb-0">Admin - Users</h1>
        <button class="btn btn-outline-primary" (click)="load()">Refresh</button>
      </div>

      <div class="card hf-card">
        <div class="card-body hf-table-wrap">
          <table class="table table-striped table-hover align-middle" *ngIf="users().length > 0; else noUsers">
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
                <td>
                  <span class="badge" [class.text-bg-success]="user.isActive" [class.text-bg-secondary]="!user.isActive">
                    {{ user.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-sm" [class.btn-outline-danger]="user.isActive" [class.btn-outline-success]="!user.isActive" (click)="toggleStatus(user)">
                    {{ user.isActive ? 'Disable' : 'Enable' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <ng-template #noUsers>
            <div class="alert alert-info mb-0">No users found.</div>
          </ng-template>
        </div>
      </div>
    </section>
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
