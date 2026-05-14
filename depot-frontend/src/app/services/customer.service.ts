import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import { CustomerResponse } from '../models/customer.model';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private readonly apiUrl = '/api/Customer';

  constructor(private readonly http: HttpClient) {}

  getCustomers(
    pageNumber = 1,
    pageSize = 100
  ): Observable<PaginationResponse<CustomerResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginationResponse<CustomerResponse>>(
      this.apiUrl,
      { params }
    );
  }
}
