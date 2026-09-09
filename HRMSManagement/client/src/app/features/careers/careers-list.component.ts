import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CareersApiService, PublicJobOpeningDto, PublicOrgDto } from '../../core/careers-api.service';

@Component({
  selector: 'app-careers-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './careers-list.component.html',
  styleUrl: './careers-list.component.scss'
})
export class CareersListComponent implements OnInit {
  orgCode = '';
  org: PublicOrgDto | null = null;
  openings: PublicJobOpeningDto[] = [];
  loading = true;
  notFound = false;

  constructor(private route: ActivatedRoute, private router: Router, private api: CareersApiService) {}

  ngOnInit(): void {
    this.orgCode = this.route.snapshot.paramMap.get('orgCode') ?? '';
    if (!this.orgCode) {
      this.notFound = true;
      this.loading = false;
      return;
    }

    this.api.getOrg(this.orgCode).subscribe({
      next: org => (this.org = org),
      error: () => (this.notFound = true)
    });

    this.api.getOpenings(this.orgCode).subscribe({
      next: data => {
        this.openings = data;
        this.loading = false;
      },
      error: () => {
        this.notFound = true;
        this.loading = false;
      }
    });
  }

  viewJob(id: number) {
    this.router.navigate(['/careers', this.orgCode, 'jobs', id]);
  }

  experienceRange(o: PublicJobOpeningDto): string {
    if (o.minExperienceYears == null && o.maxExperienceYears == null) return 'Any experience';
    if (o.minExperienceYears != null && o.maxExperienceYears != null)
      return `${o.minExperienceYears}-${o.maxExperienceYears} yrs`;
    return `${o.minExperienceYears ?? o.maxExperienceYears} yrs`;
  }
}
