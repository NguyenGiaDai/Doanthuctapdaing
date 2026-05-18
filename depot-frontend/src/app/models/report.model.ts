export interface ReportDateRequest {
  date: string;
}

export interface ContainerThroughputReportResponse {
  lineOperatorId: number | null;
  lineOperatorCode: string;
  lineOperatorName: string;

  importCount: number;
  exportCount: number;
  totalCount: number;
}

export interface ContainerYardInventoryReportResponse {
  lineOperatorId: number | null;
  lineOperatorCode: string;
  lineOperatorName: string;

  from0To10DaysCount: number;
  from10DaysOrMoreCount: number;
  totalInYardCount: number;
}
