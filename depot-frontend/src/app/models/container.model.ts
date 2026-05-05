export interface PaginationResponse<T> {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  hasPrevious: boolean;
  hasNext: boolean;
  items: T[];
}

export interface ContainerResponse {
  id: number;
  containerNumber: string;

  containerTypeId: number;
  containerTypeCode: string;
  containerTypeName: string;

  isoCode: string;
  containerSize: number;
  maximumWeight: number;
  tareWeight: number;

  lineOperatorId: number;
  lineOperatorCode: string;
  lineOperatorName: string;

  dateOfManufacture: string;
  containerOwner: string;
  containerCondition: string;
  containerClassification: string;
  currentStatus: string;
}
