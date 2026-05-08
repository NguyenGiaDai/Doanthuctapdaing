export interface ContainerPositionResponse {
  id: number;
  containerId: number;
  blockId: number;
  bay: number;
  row: number;
  tier: number;
  positionTime: string | null;
}

export interface UpdateContainerPositionRequest {
  containerId: number;
  blockId: number;
  bay: number;
  row: number;
  tier: number;
  positionTime: string | null;
}
