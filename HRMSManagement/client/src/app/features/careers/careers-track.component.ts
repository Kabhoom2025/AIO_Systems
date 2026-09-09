import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CareersApiService, TrackingStatusDto } from '../../core/careers-api.service';

@Component({
  selector: 'app-careers-track',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './careers-track.component.html',
  styleUrl: './careers-track.component.scss'
})
export class CareersTrackComponent implements OnInit {
  status: TrackingStatusDto | null = null;
  loading = true;
  notFound = false;

  constructor(private route: ActivatedRoute, private api: CareersApiService) {}

  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('token') ?? '';
    this.api.track(token).subscribe({
      next: data => {
        this.status = data;
        this.loading = false;
      },
      error: () => {
        this.notFound = true;
        this.loading = false;
      }
    });
  }

  stageLabel(stage: string): string {
    const labels: Record<string, string> = {
      Applied: 'Applied', Screening: 'Screening', Interview: 'Interview',
      Offered: 'Offer Extended', Hired: 'Hired'
    };
    return labels[stage] ?? stage;
  }
}
