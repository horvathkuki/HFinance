import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { UserProfile } from './models';

interface AdminUserListResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  items: UserProfile[];
}

@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  constructor(private readonly httpClient: HttpClient) {}

  getUsers(): Observable<AdminUserListResponse> {
    return this.httpClient.get<AdminUserListResponse>(`${API_BASE_URL}/admin/users`);
  }

  setStatus(userId: string, isActive: boolean): Observable<UserProfile> {
    return this.httpClient.put<UserProfile>(`${API_BASE_URL}/admin/users/${userId}/status`, { isActive });
  }
}
