import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import {
  CreateDeliveryOrderRequest,
  DeliveryOrderResponse,
} from '../models/delivery-order.model';

@Injectable({
  providedIn: 'root',
})
export class DeliveryOrderService {
  private readonly apiUrl = '/api/DeliveryOrder';

  constructor(private readonly http: HttpClient) {}

  getDeliveryOrders(
    pageNumber = 1,
    pageSize = 100
  ): Observable<PaginationResponse<DeliveryOrderResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginationResponse<DeliveryOrderResponse>>(
      this.apiUrl,
      { params }
    );
  }

  createDeliveryOrder(request: CreateDeliveryOrderRequest): Observable<number> {
    return this.http.post<number>(this.apiUrl, request);
  }
}
