import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgbCollapseModule, NgbToastModule } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from './auth.service';
import { UiToastService } from './ui-toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, NgbCollapseModule, NgbToastModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  isNavbarCollapsed = true;

  constructor(
    public readonly authService: AuthService,
    public readonly toastService: UiToastService,
    private readonly router: Router
  ) {}

  logout(): void {
    this.authService.logout();
    this.toastService.show('Logged out successfully.', 'info');
    void this.router.navigateByUrl('/login');
  }

  removeToast(id: number): void {
    this.toastService.remove(id);
  }
}
