import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ImportContainerRequest } from '../models/container-transaction.model';

@Injectable({
  providedIn: 'root',
})
export class ContainerTransactionService {
  private readonly apiUrl = '/api/ContainerTransaction';

  constructor(private readonly http: HttpClient) {}

  importContainer(request: ImportContainerRequest): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/import`, request);
  }
}
