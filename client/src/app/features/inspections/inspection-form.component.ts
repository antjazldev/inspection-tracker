import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InspectionService } from '../../core/inspection.service';
import { InspectionRequest, InspectionStatus } from '../../core/models';

@Component({
  selector: 'app-inspection-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './inspection-form.component.html',
})
export class InspectionFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(InspectionService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  id = signal<string | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    assetName: ['', Validators.required],
    status: [0, Validators.required],
    inspectionDate: ['', Validators.required],
    notes: [''],
  });

  isFailed(): boolean {
    return Number(this.form.controls.status.value) === InspectionStatus.Failed;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.api.getById(id).subscribe({
        next: (i) =>
          this.form.patchValue({
            assetName: i.assetName,
            status: i.status,
            inspectionDate: i.inspectionDate.slice(0, 16),
            notes: i.notes ?? '',
          }),
        error: () => this.error.set('Could not load the inspection.'),
      });
    } else {
      // sensible default: now, local
      const now = new Date();
      now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
      this.form.patchValue({ inspectionDate: now.toISOString().slice(0, 16) });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set(null);

    const v = this.form.getRawValue();
    const request: InspectionRequest = {
      assetName: v.assetName,
      status: Number(v.status),
      notes: v.notes.trim() || null,
      inspectionDate: new Date(v.inspectionDate).toISOString(),
    };

    const call = this.id()
      ? this.api.update(this.id()!, request)
      : this.api.create(request);

    call.subscribe({
      next: () => this.router.navigate(['/inspections']),
      error: (err) => {
        this.error.set(err.error?.error ?? 'Save failed.');
        this.loading.set(false);
      },
    });
  }
}