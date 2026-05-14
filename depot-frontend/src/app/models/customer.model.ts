export interface CustomerResponse {
  id: number;
  customerCode: string;
  customerName: string;
  customerTaxCode: string | null;
  address: string | null;
}
