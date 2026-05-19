export interface DeliveryOrderResponse {
  id: number;
  doNumber: string;

  customerId: number;
  customerName: string | null;

  lineOperatorId: number;
  lineOperatorName: string | null;

  containerTypeId: number;
  containerTypeName: string | null;

  quantity: number;
  expiryDate: string;
  orderDate: string | null;
  vesselVoyage: string | null;
  orderStatus: string | null;
}

export interface CreateDeliveryOrderRequest {
  doNumber: string;
  customerId: number;
  lineOperatorId: number;
  containerTypeId: number;
  quantity: number;
  expiryDate: string;
  orderDate: string | null;
  vesselVoyage: string | null;
  orderStatus: string | null;
}
