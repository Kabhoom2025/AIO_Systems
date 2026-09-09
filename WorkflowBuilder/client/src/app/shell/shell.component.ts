import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../core/auth.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, ButtonModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  userName = '';

  constructor(private auth: AuthService, private router: Router) {
    const raw = localStorage.getItem(environment.userKey);
    this.userName = raw ? JSON.parse(raw).name : '';
  }

  logout() {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
