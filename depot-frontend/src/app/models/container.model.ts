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
  maximumWeight: number | null;
  tareWeight: number | null;

  lineOperatorId: number;
  lineOperatorCode: string;
  lineOperatorName: string;

  dateOfManufacture: string | null;
  containerOwner: string;
  containerCondition: string;
  containerClassification: string | null;
  currentStatus: string | null;
}

export interface CreateContainerRequest {
  containerNumber: string;
  containerTypeId: number;
  lineOperatorId: number;
  dateOfManufacture: string | null;
  containerOwner: string;
  containerCondition: string;
  containerClassification: string | null;
  currentStatus: string | null;
}
