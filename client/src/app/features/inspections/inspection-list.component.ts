import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { InspectionService } from '../../core/inspection.service';
import { InspectionResponse, InspectionStatus } from '../../core/models';

@Component({
  selector: 'app-inspection-list',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './inspection-list.component.html'
})
export class InspectionListComponent implements OnInit {
  auth = inject(AuthService);
  private api = inject(InspectionService);

  inspections = signal<InspectionResponse[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getAll().subscribe({
      next: (data) => {
        this.inspections.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load inspections. Is the API running?');
        this.loading.set(false);
      },
    });
  }

  isOwner(item: InspectionResponse): boolean {
    return item.createdByUserId === this.auth.userId();
  }

  remove(item: InspectionResponse): void {
    if (!confirm(`Delete inspection for "${item.assetName}"?`)) return;
    this.api.delete(item.id).subscribe({
      next: () => this.inspections.update((list) => list.filter((i) => i.id !== item.id)),
      error: (err) => this.error.set(err.error?.error ?? 'Delete failed.'),
    });
  }

  statusLabel(s: InspectionStatus): string {
    return ['Pending', 'Passed', 'Failed'][s] ?? 'Unknown';
  }

  statusClass(s: InspectionStatus): string {
    return ['pending', 'passed', 'failed'][s] ?? '';
  }
}