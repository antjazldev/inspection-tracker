export enum InspectionStatus {
  Pending = 0,
  Passed = 1,
  Failed = 2,
}

export interface InspectionResponse {
  id: string;
  assetName: string;
  status: InspectionStatus;
  notes: string | null;
  inspectionDate: string;
  createdByUserId: string;
  createdByName: string;
}

export interface InspectionRequest {
  assetName: string;
  status: InspectionStatus;
  notes: string | null;
  inspectionDate: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  displayName: string;
}