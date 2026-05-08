import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import {
  ContainerPositionResponse,
  UpdateContainerPositionRequest,
} from '../models/container-position.model';

@Injectable({
  providedIn: 'root',
})
export class ContainerPositionService {
  private readonly apiUrl = '/api/ContainerPosition';

  constructor(private readonly http: HttpClient) {}

  getContainerPositions(
    pageNumber = 1,
    pageSize = 100
  ): Observable<PaginationResponse<ContainerPositionResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginationResponse<ContainerPositionResponse>>(
      this.apiUrl,
      { params }
    );
  }

  updateContainerPosition(
    id: number,
    request: UpdateContainerPositionRequest
  ): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }
}
