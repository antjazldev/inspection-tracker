import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="topbar">
      <a routerLink="/inspections" class="brand">🔍 InspectionTracker</a>
      <nav>
        @if (auth.isLoggedIn()) {
          <span class="user">{{ auth.displayName() }}</span>
          <button class="link" (click)="logout()">Log out</button>
        } @else {
          <a routerLink="/login">Log in</a>
          <a routerLink="/register">Register</a>
        }
      </nav>
    </header>
    <main>
      <router-outlet />
    </main>
  `,
})
export class AppComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/inspections']);
  }
}
