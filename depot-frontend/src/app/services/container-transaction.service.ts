import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  ContainerTransactionResponse,
  ExportContainerRequest,
  ImportContainerRequest,
} from '../models/container-transaction.model';

import { PaginationResponse } from '../models/container.model';

@Injectable({
  providedIn: 'root',
})
export class ContainerTransactionService {
  private readonly apiUrl = '/api/ContainerTransaction';

  constructor(private readonly http: HttpClient) {}

  getContainerTransactions(
    pageNumber = 1,
    pageSize = 50
  ): Observable<PaginationResponse<ContainerTransactionResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginationResponse<ContainerTransactionResponse>>(
      this.apiUrl,
      { params }
    );
  }

  importContainer(request: ImportContainerRequest): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/import`, request);
  }
  exportContainer(request: ExportContainerRequest): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/export`, request);
  }
}
