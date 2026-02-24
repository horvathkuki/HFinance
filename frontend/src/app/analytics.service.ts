import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { PortfolioAnalytics, Snapshot, SnapshotCreateResponse, TimeSeriesPoint } from './models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  constructor(private readonly httpClient: HttpClient) {}

  getPortfolioAnalytics(portfolioId: number): Observable<PortfolioAnalytics> {
    return this.httpClient.get<PortfolioAnalytics>(`${API_BASE_URL}/analytics/portfolio/${portfolioId}`);
  }

  getTimeSeries(portfolioId: number): Observable<TimeSeriesPoint[]> {
    return this.httpClient.get<TimeSeriesPoint[]>(
      `${API_BASE_URL}/analytics/portfolio/${portfolioId}/timeseries`
    );
  }

  createSnapshot(portfolioId: number): Observable<SnapshotCreateResponse> {
    return this.httpClient.post<SnapshotCreateResponse>(`${API_BASE_URL}/portfolios/${portfolioId}/snapshots`, {});
  }

  getSnapshots(portfolioId: number): Observable<Snapshot[]> {
    return this.httpClient.get<Snapshot[]>(`${API_BASE_URL}/portfolios/${portfolioId}/snapshots`);
  }
}
