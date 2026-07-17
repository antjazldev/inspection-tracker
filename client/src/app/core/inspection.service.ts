import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { InspectionRequest, InspectionResponse } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InspectionService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/inspections`;

  getAll() {
    return this.http.get<InspectionResponse[]>(this.base);
  }

  getById(id: string) {
    return this.http.get<InspectionResponse>(`${this.base}/${id}`);
  }

  create(request: InspectionRequest) {
    return this.http.post<InspectionResponse>(this.base, request);
  }

  update(id: string, request: InspectionRequest) {
    return this.http.put<InspectionResponse>(`${this.base}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}