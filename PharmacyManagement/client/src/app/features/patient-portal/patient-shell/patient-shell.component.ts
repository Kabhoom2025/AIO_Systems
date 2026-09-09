import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { PatientAuthService, PatientLoginResponse } from '../../../core/patient-auth.service';
import { PatientCartService } from '../../../core/patient-cart.service';

@Component({
  selector: 'app-patient-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './patient-shell.component.html',
  styleUrl: './patient-shell.component.scss'
})
export class PatientShellComponent {
  user: PatientLoginResponse | null;
  menuOpen = signal(false);

  constructor(private auth: PatientAuthService, public cart: PatientCartService, private router: Router) {
    this.user = this.auth.getUser();
  }

  toggleMenu() {
    this.menuOpen.set(!this.menuOpen());
  }

  logout() {
    this.auth.logout();
    this.router.navigateByUrl('/patient/login');
  }
}
