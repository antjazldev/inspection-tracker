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
  template: `
    <div class="list-header">
      <h1>Inspections</h1>
      @if (auth.isLoggedIn()) {
        <a routerLink="/inspections/new"><button>+ New inspection</button></a>
      }
    </div>

    @if (loading()) {
      <p>Loading...</p>
    } @else if (inspections().length === 0) {
      <p>No inspections yet.</p>
    } @else {
      @for (item of inspections(); track item.id) {
        <div class="card">
          <div>
            <strong>{{ item.assetName }}</strong>
            <div class="meta">
              {{ item.inspectionDate | date: 'medium' }} · by {{ item.createdByName }}
            </div>
          </div>

          <span class="badge" [class]="'badge ' + statusClass(item.status)">
            {{ statusLabel(item.status) }}
          </span>

          @if (isOwner(item)) {
            <div class="actions">
              <a [routerLink]="['/inspections', item.id, 'edit']">
                <button class="link">Edit</button>
              </a>
              <button class="link danger-text" (click)="remove(item)">Delete</button>
            </div>
          }

          @if (item.notes) {
            <p class="notes">{{ item.notes }}</p>
          }
        </div>
      }
    }

    @if (error()) {
      <p class="error">{{ error() }}</p>
    }
  `
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