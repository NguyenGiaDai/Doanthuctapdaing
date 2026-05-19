import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import { BlockResponse, CreateBlockRequest } from '../models/block.model';

@Injectable({
  providedIn: 'root',
})
export class BlockService {
  private readonly apiUrl = '/api/Block';

  constructor(private readonly http: HttpClient) {}

  getBlocks(pageNumber = 1, pageSize = 100): Observable<PaginationResponse<BlockResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginationResponse<BlockResponse>>(this.apiUrl, { params });
  }

  createBlock(request: CreateBlockRequest): Observable<number> {
    return this.http.post<number>(this.apiUrl, request);
  }
}
