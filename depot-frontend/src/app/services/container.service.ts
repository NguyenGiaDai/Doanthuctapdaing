import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ContainerResponse, PaginationResponse } from '../models/container.model';

@Injectable({
  providedIn: 'root',
})
export class ContainerService {
  private readonly apiUrl = '/api/Container';

  constructor(private http: HttpClient) {}

  getContainers(pageIndex = 0, pageSize = 10): Observable<PaginationResponse<ContainerResponse>> {
    return this.http.get<PaginationResponse<ContainerResponse>>(
      `${this.apiUrl}?pageIndex=${pageIndex}&pageSize=${pageSize}`
    );
  }
}
