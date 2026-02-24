import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-account-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page">
      <h1>Account Settings</h1>

      <form [formGroup]="profileForm" (ngSubmit)="saveProfile()">
        <h2>Profile</h2>
        <label>Email <input type="email" formControlName="email" /></label>
        <label>Display Name <input type="text" formControlName="displayName" /></label>
        <label>
          Base Currency
          <select formControlName="baseCurrency">
            <option value="EUR">EUR</option>
            <option value="USD">USD</option>
            <option value="RON">RON</option>
          </select>
        </label>
        <button type="submit">Save Profile</button>
      </form>

      <form [formGroup]="passwordForm" (ngSubmit)="changePassword()">
        <h2>Password</h2>
        <label>Current Password <input type="password" formControlName="currentPassword" /></label>
        <label>New Password <input type="password" formControlName="newPassword" /></label>
        <button type="submit">Change Password</button>
      </form>

      <p>{{ message() }}</p>
    </section>
  `,
  styles: `
    .page { max-width: 700px; margin: 2rem auto; display: grid; gap: 1.25rem; }
    form { border: 1px solid #d0d0d0; padding: 1rem; display: grid; gap: 0.5rem; }
    label { display: grid; gap: 0.2rem; }
    input, select { padding: 0.45rem; }
  `,
})
export class AccountPage {
  readonly message = signal('');
  private readonly formBuilder = inject(FormBuilder);

  readonly profileForm = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    baseCurrency: ['EUR', [Validators.required]],
  });

  readonly passwordForm = this.formBuilder.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor(
    private readonly authService: AuthService
  ) {
    this.authService.getMe().subscribe({
      next: (profile) => {
        this.profileForm.patchValue({
          email: profile.email,
          displayName: profile.displayName,
          baseCurrency: profile.baseCurrency,
        });
      },
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      return;
    }
    const value = this.profileForm.getRawValue();
    this.authService
      .updateProfile(value.email ?? '', value.displayName ?? '', value.baseCurrency ?? 'EUR')
      .subscribe({
        next: () => this.message.set('Profile updated.'),
        error: () => this.message.set('Could not update profile.'),
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      return;
    }
    const value = this.passwordForm.getRawValue();
    this.authService
      .changePassword(value.currentPassword ?? '', value.newPassword ?? '')
      .subscribe({
        next: () => this.message.set('Password updated.'),
        error: () => this.message.set('Could not update password.'),
      });
  }
}
