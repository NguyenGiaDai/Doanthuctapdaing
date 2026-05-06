import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PaginationResponse } from '../models/container.model';
import { ContainerTypeResponse } from '../models/container-type.model';

@Injectable({
  providedIn: 'root',
})
export class ContainerTypeService {
  private readonly apiUrl = '/api/ContainerType';

  constructor(private readonly http: HttpClient) {}

  getContainerTypes(pageNumber = 1, pageSize = 50): Observable<PaginationResponse<ContainerTypeResponse>> {
    return this.http.get<PaginationResponse<ContainerTypeResponse>>(
      `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  }
}
