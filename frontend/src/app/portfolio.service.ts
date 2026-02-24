import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { Holding, HoldingGroup, Portfolio } from './models';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  constructor(private readonly httpClient: HttpClient) {}

  getPortfolios(): Observable<Portfolio[]> {
    return this.httpClient.get<Portfolio[]>(`${API_BASE_URL}/portfolios`);
  }

  createPortfolio(name: string, description?: string): Observable<Portfolio> {
    return this.httpClient.post<Portfolio>(`${API_BASE_URL}/portfolios`, { name, description });
  }

  updatePortfolio(id: number, name: string, description?: string): Observable<void> {
    return this.httpClient.put<void>(`${API_BASE_URL}/portfolios/${id}`, { name, description });
  }

  deletePortfolio(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${API_BASE_URL}/portfolios/${id}`);
  }

  getHoldings(portfolioId: number): Observable<Holding[]> {
    return this.httpClient.get<Holding[]>(`${API_BASE_URL}/portfolios/${portfolioId}/holdings`);
  }

  addHolding(
    portfolioId: number,
    payload: {
      symbol: string;
      quantity: number;
      averagePurchasePrice: number;
      currency: string;
      groupId: number;
      purchaseDate?: string;
    }
  ): Observable<Holding> {
    return this.httpClient.post<Holding>(`${API_BASE_URL}/portfolios/${portfolioId}/holdings`, payload);
  }

  updateHolding(
    portfolioId: number,
    holdingId: number,
    payload: {
      symbol: string;
      quantity: number;
      averagePurchasePrice: number;
      currency: string;
      groupId: number;
      purchaseDate?: string;
    }
  ): Observable<void> {
    return this.httpClient.put<void>(
      `${API_BASE_URL}/portfolios/${portfolioId}/holdings/${holdingId}`,
      payload
    );
  }

  deleteHolding(portfolioId: number, holdingId: number): Observable<void> {
    return this.httpClient.delete<void>(`${API_BASE_URL}/portfolios/${portfolioId}/holdings/${holdingId}`);
  }
}
