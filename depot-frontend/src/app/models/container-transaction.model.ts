export interface ImportContainerRequest {
  containerId: number;
  toBlockId: number;
  toBay: number;
  toRow: number;
  toTier: number;
  vehicleNumber: string | null;
  transactionTime: string | null;
  note: string | null;
}

export interface ExportContainerRequest {
  containerId: number;
  deliveryOrderId: number;
  vehicleNumber: string | null;
  transactionTime: string | null;
  note: string | null;
}

export interface CreateContainerTransactionRequest {
  containerId: number;
  transactionType: string;

  fromBlockId: number | null;
  fromBay: number | null;
  fromRow: number | null;
  fromTier: number | null;

  toBlockId: number | null;
  toBay: number | null;
  toRow: number | null;
  toTier: number | null;

  vehicleNumber: string | null;
  transactionTime: string | null;
  note: string | null;
}

export interface ContainerTransactionResponse {
  id: number;
  containerId: number;
  transactionType: string;

  fromBlockId: number | null;
  fromBay: number | null;
  fromRow: number | null;
  fromTier: number | null;

  toBlockId: number | null;
  toBay: number | null;
  toRow: number | null;
  toTier: number | null;

  vehicleNumber: string | null;
  transactionTime: string | null;
  note: string | null;
}
