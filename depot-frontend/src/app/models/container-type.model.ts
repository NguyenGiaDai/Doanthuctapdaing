export interface ContainerTypeResponse {
  id: number;
  containerTypeCode: string;
  containerTypeName: string;
  isoCode: string;
  containerSize: number;
  maximumWeight: number | null;
  tareWeight: number | null;
}

export interface CreateContainerTypeRequest {
  containerTypeCode: string;
  containerTypeName: string;
  isoCode: string;
  containerSize: number;
  maximumWeight: number | null;
  tareWeight: number | null;
}
