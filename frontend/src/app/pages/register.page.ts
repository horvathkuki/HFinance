import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page">
      <h1>Create account</h1>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>Display Name <input type="text" formControlName="displayName" /></label>
        <label>Email <input type="email" formControlName="email" /></label>
        <label>Password <input type="password" formControlName="password" /></label>
        <button type="submit" [disabled]="form.invalid || loading()">Register</button>
      </form>
      <p class="error" *ngIf="errorMessage()">{{ errorMessage() }}</p>
      <a routerLink="/login">Already have an account?</a>
    </section>
  `,
  styles: `
    .page { max-width: 420px; margin: 2rem auto; display: grid; gap: 1rem; }
    form { display: grid; gap: 0.75rem; }
    label { display: grid; gap: 0.25rem; }
    input { padding: 0.5rem; }
    .error { color: #a80000; }
  `,
})
export class RegisterPage {
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  private readonly formBuilder = inject(FormBuilder);

  readonly form = this.formBuilder.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    const { displayName, email, password } = this.form.getRawValue();

    this.authService.register(email ?? '', password ?? '', displayName ?? '').subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigateByUrl('/dashboard');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Registration failed.');
      },
    });
  }
}
