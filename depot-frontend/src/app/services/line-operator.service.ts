import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import {
  CreateLineOperatorRequest,
  LineOperatorResponse,
} from '../models/line-operator.model';

@Injectable({
  providedIn: 'root',
})
export class LineOperatorService {
  private readonly apiUrl = '/api/LineOperator';

  constructor(private readonly http: HttpClient) {}

  getLineOperators(pageNumber = 1, pageSize = 50): Observable<PaginationResponse<LineOperatorResponse>> {
    return this.http.get<PaginationResponse<LineOperatorResponse>>(
      `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  }

  createLineOperator(request: CreateLineOperatorRequest): Observable<number> {
    return this.http.post<number>(this.apiUrl, request);
  }
}
