import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page">
      <h1>Sign in</h1>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>Email <input type="email" formControlName="email" /></label>
        <label>Password <input type="password" formControlName="password" /></label>
        <button type="submit" [disabled]="form.invalid || loading()">Login</button>
      </form>
      <p class="error" *ngIf="errorMessage()">{{ errorMessage() }}</p>
      <a routerLink="/register">Create account</a>
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
export class LoginPage {
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  private readonly formBuilder = inject(FormBuilder);

  readonly form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
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
    const { email, password } = this.form.getRawValue();

    this.authService.login(email ?? '', password ?? '').subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigateByUrl('/dashboard');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Login failed. Check your credentials.');
      },
    });
  }
}
