import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  ContainerThroughputReportResponse,
  ContainerYardInventoryReportResponse,
  ReportDateRequest,
} from '../models/report.model';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private readonly apiUrl = '/api/ContainerTransaction';

  constructor(private readonly http: HttpClient) {}

  getThroughputReport(
    request: ReportDateRequest
  ): Observable<ContainerThroughputReportResponse[]> {
    return this.http.post<ContainerThroughputReportResponse[]>(
      `${this.apiUrl}/report/throughput-by-line-operator`,
      request
    );
  }

  getYardInventoryReport(
    request: ReportDateRequest
  ): Observable<ContainerYardInventoryReportResponse[]> {
    return this.http.post<ContainerYardInventoryReportResponse[]>(
      `${this.apiUrl}/report/yard-inventory-by-line-operator`,
      request
    );
  }
}
