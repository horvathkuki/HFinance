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
    <section class="hf-page d-flex justify-content-center align-items-start pt-5">
      <div class="card hf-card w-100" style="max-width: 460px;">
        <div class="card-body p-4">
          <h1 class="h4 mb-3">Sign in</h1>
          <form [formGroup]="form" (ngSubmit)="submit()" class="row g-3">
            <div class="col-12">
              <label class="form-label">Email</label>
              <input class="form-control" type="email" formControlName="email" />
            </div>
            <div class="col-12">
              <label class="form-label">Password</label>
              <input class="form-control" type="password" formControlName="password" />
            </div>
            <div class="col-12 d-grid">
              <button class="btn btn-primary" type="submit" [disabled]="form.invalid || loading()">
                Login
              </button>
            </div>
          </form>
          <div class="alert alert-danger mt-3 mb-0" *ngIf="errorMessage()">{{ errorMessage() }}</div>
          <p class="mt-3 mb-0">
            No account? <a routerLink="/register">Create one</a>
          </p>
        </div>
      </div>
    </section>
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
