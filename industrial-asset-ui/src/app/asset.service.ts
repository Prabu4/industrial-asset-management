
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ApiAsset {
  id: number;
  assetCode: string;
  equipment: string;
  area: string;
  lastMaintenance: string;
  status: 'Operational' | 'Maintenance' | 'Out of Service';
}

export interface CreateAssetRequest {
  assetCode: string;
  equipment: string;
  area: string;
  lastMaintenance: string;
  status: 'Operational' | 'Maintenance' | 'Out of Service';
}

@Injectable({
  providedIn: 'root'
})
export class AssetService {
  private http = inject(HttpClient);

  private readonly apiUrl = 'http://localhost:5018/api/Assets';

  getAssets(): Observable<ApiAsset[]> {
    return this.http.get<ApiAsset[]>(this.apiUrl);
  }

  createAsset(asset: CreateAssetRequest): Observable<ApiAsset> {
    return this.http.post<ApiAsset>(this.apiUrl, asset);
  }
  
updateAsset(
  id: number,
  asset: CreateAssetRequest
): Observable<void> {
  return this.http.put<void>(`${this.apiUrl}/${id}`, {
    id,
    ...asset
  });
}

deleteAsset(id: number): Observable<void> {
  return this.http.delete<void>(`${this.apiUrl}/${id}`);
}

}
