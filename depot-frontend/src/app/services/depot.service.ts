import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import { DepotResponse } from '../models/depot.model';

@Injectable({
  providedIn: 'root',
})
export class DepotService {
  private readonly apiUrl = '/api/Depot';

  constructor(private readonly http: HttpClient) {}

  getDepots(pageIndex = 1, pageSize = 100): Observable<PaginationResponse<DepotResponse>> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex)
      .set('pageSize', pageSize);

    return this.http.get<PaginationResponse<DepotResponse>>(this.apiUrl, { params });
  }
}
