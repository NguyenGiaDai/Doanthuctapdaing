export interface BlockResponse {
  id: number;
  depotId: number;
  depotCode: string;
  depotName: string;
  blockCode: string;
  blockName: string;
  blockType: string;
  maxBay: number | null;
  maxRow: number | null;
  maxTier: number | null;
}

export interface CreateBlockRequest {
  depotId: number;
  blockCode: string;
  blockName: string;
  blockType: string;
  maxBay: number | null;
  maxRow: number | null;
  maxTier: number | null;
}
