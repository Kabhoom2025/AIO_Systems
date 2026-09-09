import { Component } from '@angular/core';
import { PatientAuthService, PatientLoginResponse } from '../../../core/patient-auth.service';

@Component({
  selector: 'app-patient-home',
  standalone: true,
  imports: [],
  templateUrl: './patient-home.component.html',
  styleUrl: './patient-home.component.scss'
})
export class PatientHomeComponent {
  user: PatientLoginResponse | null;

  constructor(private auth: PatientAuthService) {
    this.user = this.auth.getUser();
  }
}
