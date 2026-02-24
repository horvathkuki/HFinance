import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { HoldingGroup } from './models';

@Injectable({ providedIn: 'root' })
export class GroupsService {
  constructor(private readonly httpClient: HttpClient) {}

  getGroups(): Observable<HoldingGroup[]> {
    return this.httpClient.get<HoldingGroup[]>(`${API_BASE_URL}/groups`);
  }

  createGroup(name: string, description?: string): Observable<HoldingGroup> {
    return this.httpClient.post<HoldingGroup>(`${API_BASE_URL}/groups`, { name, description });
  }

  updateGroup(id: number, name: string, description?: string): Observable<void> {
    return this.httpClient.put<void>(`${API_BASE_URL}/groups/${id}`, { name, description });
  }

  deleteGroup(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${API_BASE_URL}/groups/${id}`);
  }
}
