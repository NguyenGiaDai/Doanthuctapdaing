export interface LineOperatorResponse {
  id: number;
  lineOperatorCode: string;
  lineOperatorName: string;
}

export interface CreateLineOperatorRequest {
  lineOperatorCode: string;
  lineOperatorName: string;
}
