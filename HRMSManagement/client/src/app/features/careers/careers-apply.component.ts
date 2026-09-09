import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApplyToJobDto, CareersApiService, PublicJobOpeningDetailDto } from '../../core/careers-api.service';

const SOURCE_OPTIONS = ['LinkedIn', 'Naukri', 'CompanyWebsite', 'Referral', 'Other'];

@Component({
  selector: 'app-careers-apply',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './careers-apply.component.html',
  styleUrl: './careers-apply.component.scss'
})
export class CareersApplyComponent implements OnInit {
  orgCode = '';
  openingId = 0;
  opening: PublicJobOpeningDetailDto | null = null;
  loading = true;
  notFound = false;

  sourceOptions = SOURCE_OPTIONS;
  showApplyForm = false;
  submitting = false;
  submitted = false;
  trackingToken: string | null = null;
  resultMessage = '';

  form: ApplyToJobDto = this.emptyForm();

  constructor(private route: ActivatedRoute, private router: Router, private api: CareersApiService) {}

  ngOnInit(): void {
    this.orgCode = this.route.snapshot.paramMap.get('orgCode') ?? '';
    this.openingId = Number(this.route.snapshot.paramMap.get('id'));

    this.api.getOpening(this.orgCode, this.openingId).subscribe({
      next: data => {
        this.opening = data;
        this.loading = false;
      },
      error: () => {
        this.notFound = true;
        this.loading = false;
      }
    });
  }

  experienceRange(): string {
    if (!this.opening) return '';
    const { minExperienceYears: min, maxExperienceYears: max } = this.opening;
    if (min == null && max == null) return 'Any experience';
    if (min != null && max != null) return `${min}-${max} yrs`;
    return `${min ?? max} yrs`;
  }

  salaryRange(): string | null {
    if (!this.opening) return null;
    const { salaryRangeFrom: from, salaryRangeTo: to } = this.opening;
    if (from == null && to == null) return null;
    const fmt = (v: number) => `₹${(v / 100000).toFixed(1)}L`;
    if (from != null && to != null) return `${fmt(from)} - ${fmt(to)} per annum`;
    return `${fmt(from ?? to!)} per annum`;
  }

  submit() {
    if (!this.form.name.trim() || !this.form.email.trim()) return;
    this.submitting = true;
    this.api.apply(this.orgCode, this.openingId, this.form).subscribe({
      next: res => {
        this.submitting = false;
        this.submitted = true;
        this.trackingToken = res.trackingToken;
        this.resultMessage = res.message;
      },
      error: err => {
        this.submitting = false;
        this.resultMessage = err.error?.message ?? 'Something went wrong. Please try again.';
      }
    });
  }

  trackingUrl(): string {
    return `${window.location.origin}/careers/track/${this.trackingToken}`;
  }

  copyTrackingLink() {
    navigator.clipboard?.writeText(this.trackingUrl());
  }

  private emptyForm(): ApplyToJobDto {
    return {
      name: '', email: '', phone: '', currentCompany: '',
      totalExperienceYears: null, expectedSalary: null, resumeUrl: '',
      source: 'LinkedIn', coverNote: ''
    };
  }
}
