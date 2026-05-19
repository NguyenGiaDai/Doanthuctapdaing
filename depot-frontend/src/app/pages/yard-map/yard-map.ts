import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { BlockResponse } from '../../models/block.model';
import {
  ContainerPositionResponse,
  UpdateContainerPositionRequest,
} from '../../models/container-position.model';
import { ContainerResponse } from '../../models/container.model';
import { CreateContainerTransactionRequest } from '../../models/container-transaction.model';

import { BlockService } from '../../services/block.service';
import { ContainerPositionService } from '../../services/container-position.service';
import { ContainerService } from '../../services/container.service';
import { ContainerTransactionService } from '../../services/container-transaction.service';

interface YardSlotSelection {
  block: BlockResponse;
  bay: number;
  row: number;
  tier: number;
  position: ContainerPositionResponse | null;
  container: ContainerResponse | null;
}

@Component({
  selector: 'app-yard-map',
  imports: [CommonModule, FormsModule],
  templateUrl: './yard-map.html',
  styleUrl: './yard-map.scss',
})
export class YardMap implements OnInit {
  blocks: BlockResponse[] = [];
  containers: ContainerResponse[] = [];
  positions: ContainerPositionResponse[] = [];

  selectedBlockId: number | null = null;
  selectedTier = 1;
  selectedSlot: YardSlotSelection | null = null;

  moveSourceSlot: YardSlotSelection | null = null;
  moveTargetSlot: YardSlotSelection | null = null;

  isLoading = false;
  isMoveSubmitting = false;

  errorMessage = '';
  moveSuccessMessage = '';
  moveErrorMessage = '';
  lastUpdatedAt: Date | null = null;

  private readonly pageSize = 500;

  constructor(
    private readonly blockService: BlockService,
    private readonly containerService: ContainerService,
    private readonly containerPositionService: ContainerPositionService,
    private readonly containerTransactionService: ContainerTransactionService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadYardMap();
  }

  loadYardMap(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.changeDetectorRef.detectChanges();

    forkJoin({
      blocks: this.blockService.getBlocks(1, this.pageSize),
      containers: this.containerService.getContainers(1, this.pageSize),
      positions: this.containerPositionService.getContainerPositions(1, this.pageSize),
    }).subscribe({
      next: (response) => {
        this.blocks = response.blocks.items ?? [];
        this.containers = response.containers.items ?? [];
        this.positions = response.positions.items ?? [];

        if (!this.selectedBlockId || !this.blocks.some((block) => block.id === this.selectedBlockId)) {
          this.selectedBlockId = this.getDefaultSelectedBlockId();
        }

        this.normalizeSelectedTier();

        this.selectedSlot = null;
        this.lastUpdatedAt = new Date();
        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load yard map failed:', error);

        this.errorMessage = this.getApiErrorMessage(
          error,
          'Không tải được Yard Map. Hãy kiểm tra backend Docker, proxy hoặc API Block / Container / ContainerPosition.'
        );

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  selectBlock(block: BlockResponse): void {
    this.selectedBlockId = block.id;
    this.selectedTier = 1;
    this.selectedSlot = null;
    this.changeDetectorRef.detectChanges();
  }

  selectTier(tier: number): void {
    this.selectedTier = tier;
    this.selectedSlot = null;
    this.changeDetectorRef.detectChanges();
  }

  selectSlot(block: BlockResponse, bay: number, row: number, tier: number): void {
    const position = this.getPositionAt(block.id, bay, row, tier);
    const container = position ? this.getContainerById(position.containerId) : null;

    const slot: YardSlotSelection = {
      block,
      bay,
      row,
      tier,
      position,
      container,
    };

    this.selectedSlot = slot;
    this.handleMapMoveSelection(slot);

    this.changeDetectorRef.detectChanges();
  }

  handleMapMoveSelection(slot: YardSlotSelection): void {
    if (this.isMoveSubmitting) {
      return;
    }

    if (slot.position && slot.container) {
      if (slot.container.currentStatus !== 'InYard') {
        this.moveErrorMessage = 'Container phải đang ở trong bãi mới được di chuyển.';
        this.moveSuccessMessage = '';
        return;
      }

      this.moveSourceSlot = { ...slot };
      this.moveTargetSlot = null;
      this.moveErrorMessage = '';
      this.moveSuccessMessage = '';
      return;
    }

    if (!slot.position && this.moveSourceSlot) {
      if (this.isSameSlot(this.moveSourceSlot, slot)) {
        this.moveErrorMessage = 'Ô đích phải khác ô nguồn.';
        this.moveSuccessMessage = '';
        return;
      }

      this.moveTargetSlot = { ...slot };
      this.moveErrorMessage = '';
      this.moveSuccessMessage = '';
      return;
    }

    if (!slot.position && !this.moveSourceSlot) {
      this.moveErrorMessage = 'Hãy chọn ô đang có container trước, sau đó chọn ô trống làm đích.';
      this.moveSuccessMessage = '';
    }
  }

  clearMoveSelection(): void {
    this.moveSourceSlot = null;
    this.moveTargetSlot = null;
    this.selectedSlot = null;
    this.moveSuccessMessage = '';
    this.moveErrorMessage = '';
    this.changeDetectorRef.detectChanges();
  }

  submitMove(): void {
    this.moveSuccessMessage = '';
    this.moveErrorMessage = '';

    const validationMessage = this.validateMoveAction();

    if (validationMessage) {
      this.moveErrorMessage = validationMessage;
      this.changeDetectorRef.detectChanges();
      return;
    }

    const sourceSlot = this.moveSourceSlot as YardSlotSelection;
    const targetSlot = this.moveTargetSlot as YardSlotSelection;
    const sourcePosition = sourceSlot.position as ContainerPositionResponse;
    const sourceContainer = sourceSlot.container as ContainerResponse;
    const moveTime = this.getCurrentDateTimeInputValue();

    const updatePositionRequest: UpdateContainerPositionRequest = {
      containerId: sourceContainer.id,
      blockId: targetSlot.block.id,
      bay: targetSlot.bay,
      row: targetSlot.row,
      tier: targetSlot.tier,
      positionTime: moveTime,
    };

    const createTransactionRequest: CreateContainerTransactionRequest = {
      containerId: sourceContainer.id,
      transactionType: 'Move',

      fromBlockId: sourcePosition.blockId,
      fromBay: sourcePosition.bay,
      fromRow: sourcePosition.row,
      fromTier: sourcePosition.tier,

      toBlockId: targetSlot.block.id,
      toBay: targetSlot.bay,
      toRow: targetSlot.row,
      toTier: targetSlot.tier,

      vehicleNumber: null,
      transactionTime: moveTime,
      note: 'Di chuyển container từ Yard Map',
    };

    this.isMoveSubmitting = true;
    this.changeDetectorRef.detectChanges();

    this.containerPositionService
      .updateContainerPosition(sourcePosition.id, updatePositionRequest)
      .subscribe({
        next: () => {
          this.containerTransactionService.createTransaction(createTransactionRequest).subscribe({
            next: (transactionId) => {
              this.selectedBlockId = targetSlot.block.id;
              this.selectedTier = targetSlot.tier;
              this.selectedSlot = null;
              this.moveSourceSlot = null;
              this.moveTargetSlot = null;

              this.moveSuccessMessage = `Di chuyển container thành công. Mã giao dịch: ${transactionId}.`;
              this.moveErrorMessage = '';
              this.isMoveSubmitting = false;

              this.loadYardMap();
            },
            error: (error) => {
              console.error('Create move transaction failed:', error);

              this.moveErrorMessage = this.getApiErrorMessage(
                error,
                'Đã cập nhật vị trí nhưng không ghi được lịch sử di chuyển. Hãy kiểm tra API ContainerTransaction.'
              );

              this.moveSuccessMessage = '';
              this.isMoveSubmitting = false;
              this.loadYardMap();
            },
          });
        },
        error: (error) => {
          console.error('Move container failed:', error);

          this.moveErrorMessage = this.getApiErrorMessage(
            error,
            'Không di chuyển được container. Hãy kiểm tra vị trí đích hoặc quy tắc nghiệp vụ.'
          );

          this.moveSuccessMessage = '';
          this.isMoveSubmitting = false;
          this.changeDetectorRef.detectChanges();
        },
      });
  }

  validateMoveAction(): string {
    if (!this.moveSourceSlot || !this.moveSourceSlot.position || !this.moveSourceSlot.container) {
      return 'Vui lòng chọn ô nguồn có container.';
    }

    if (this.moveSourceSlot.container.currentStatus !== 'InYard') {
      return 'Container phải đang ở trong bãi mới được di chuyển.';
    }

    if (!this.moveTargetSlot) {
      return 'Vui lòng chọn ô trống làm đích.';
    }

    if (this.moveTargetSlot.position) {
      return 'Ô đích phải là ô trống.';
    }

    if (this.isSameSlot(this.moveSourceSlot, this.moveTargetSlot)) {
      return 'Ô đích phải khác ô nguồn.';
    }

    return '';
  }

  getSelectedBlock(): BlockResponse | undefined {
    if (!this.selectedBlockId) {
      return undefined;
    }

    return this.blocks.find((block) => block.id === this.selectedBlockId);
  }

  getPhysicalBlocks(): BlockResponse[] {
    return this.blocks.filter((block) => !this.isVirtualBlock(block));
  }

  getVirtualBlocks(): BlockResponse[] {
    return this.blocks.filter((block) => this.isVirtualBlock(block));
  }

  getTotalCapacity(): number {
    return this.getPhysicalBlocks().reduce((total, block) => total + this.getBlockCapacity(block), 0);
  }

  getTotalOccupiedSlots(): number {
    return this.positions.length;
  }

  getTotalUsagePercent(): number {
    const capacity = this.getTotalCapacity();

    if (capacity <= 0) {
      return 0;
    }

    return Math.round((this.getTotalOccupiedSlots() / capacity) * 100);
  }

  getBlockCapacity(block: BlockResponse): number {
    if (this.isVirtualBlock(block)) {
      return 0;
    }

    return (block.maxBay ?? 0) * (block.maxRow ?? 0) * (block.maxTier ?? 0);
  }

  getBlockOccupiedSlots(block: BlockResponse): number {
    return this.positions.filter((position) => position.blockId === block.id).length;
  }

  getBlockOccupiedSlotsByTier(block: BlockResponse, tier: number): number {
    return this.positions.filter(
      (position) => position.blockId === block.id && position.tier === tier
    ).length;
  }

  getBlockUsagePercent(block: BlockResponse): number {
    const capacity = this.getBlockCapacity(block);

    if (capacity <= 0) {
      return 0;
    }

    return Math.round((this.getBlockOccupiedSlots(block) / capacity) * 100);
  }

  getCurrentTierCapacity(block: BlockResponse): number {
    if (this.isVirtualBlock(block)) {
      return 0;
    }

    return (block.maxBay ?? 0) * (block.maxRow ?? 0);
  }

  getCurrentTierUsagePercent(block: BlockResponse): number {
    const capacity = this.getCurrentTierCapacity(block);

    if (capacity <= 0) {
      return 0;
    }

    return Math.round((this.getBlockOccupiedSlotsByTier(block, this.selectedTier) / capacity) * 100);
  }

  getBayNumbers(block: BlockResponse): number[] {
    return this.createNumberRange(block.maxBay ?? 0);
  }

  getRowNumbers(block: BlockResponse): number[] {
    return this.createNumberRange(block.maxRow ?? 0);
  }

  getTierNumbers(block: BlockResponse): number[] {
    return this.createNumberRange(block.maxTier ?? 0);
  }

  getPlanGridTemplateColumns(block: BlockResponse): string {
    const bayCount = block.maxBay ?? 0;

    if (bayCount <= 0) {
      return '72px';
    }

    return `72px repeat(${bayCount}, minmax(72px, 1fr))`;
  }

  getPositionAt(
    blockId: number,
    bay: number,
    row: number,
    tier: number
  ): ContainerPositionResponse | null {
    return (
      this.positions.find(
        (position) =>
          position.blockId === blockId &&
          position.bay === bay &&
          position.row === row &&
          position.tier === tier
      ) ?? null
    );
  }

  getContainerById(containerId: number): ContainerResponse | null {
    return this.containers.find((container) => container.id === containerId) ?? null;
  }

  getContainerDisplayName(container: ContainerResponse | null | undefined): string {
    if (!container) {
      return 'Không có thông tin container';
    }

    return container.containerNumber || `Container ${container.id}`;
  }

  getShortContainerNumber(containerNumber: string | null | undefined): string {
    if (!containerNumber) {
      return 'N/A';
    }

    if (containerNumber.length <= 12) {
      return containerNumber;
    }

    return `${containerNumber.slice(0, 10)}...`;
  }

  getContainerTypeDisplay(container: ContainerResponse | null | undefined): string {
    if (!container) {
      return 'N/A';
    }

    return container.containerTypeCode || container.containerTypeName || 'N/A';
  }

  getContainerLineDisplay(container: ContainerResponse | null | undefined): string {
    if (!container) {
      return 'N/A';
    }

    return container.lineOperatorCode || container.lineOperatorName || 'N/A';
  }

  getContainerConditionDisplay(condition: string | null | undefined): string {
    const normalizedCondition = condition?.trim().toLowerCase();

    if (normalizedCondition === 'normal' || normalizedCondition === 'good') {
      return 'Good';
    }

    if (normalizedCondition === 'damaged') {
      return 'Damaged';
    }

    if (normalizedCondition === 'inspection') {
      return 'Inspection';
    }

    return condition || 'N/A';
  }

  getBlockDisplayName(block: BlockResponse): string {
    return block.blockCode || block.blockName || `Block ${block.id}`;
  }

  getBlockTypeDisplayName(blockType: string | null | undefined): string {
    const normalizedBlockType = blockType?.trim().toLowerCase();

    if (normalizedBlockType === 'normal') {
      return 'Normal';
    }

    if (normalizedBlockType === 'special') {
      return 'Special';
    }

    if (normalizedBlockType === 'electric') {
      return 'Electric';
    }

    if (normalizedBlockType === 'damaged') {
      return 'Damaged';
    }

    if (normalizedBlockType === 'virtual') {
      return 'Virtual';
    }

    return blockType || 'Không xác định';
  }

  getBlockTypeHint(blockType: string | null | undefined): string {
    const normalizedBlockType = blockType?.trim().toLowerCase();

    if (normalizedBlockType === 'normal') {
      return 'Nhận container phân loại A';
    }

    if (normalizedBlockType === 'special') {
      return 'Nhận container phân loại B';
    }

    if (normalizedBlockType === 'electric') {
      return 'Nhận container lạnh / phân loại C';
    }

    if (normalizedBlockType === 'damaged') {
      return 'Nhận container hư hỏng';
    }

    if (normalizedBlockType === 'virtual') {
      return 'Block ảo, không kiểm soát Bay / Row / Tier';
    }

    return 'Chưa xác định loại block';
  }

  getBlockTypeClass(blockType: string | null | undefined): string {
    const normalizedBlockType = blockType?.trim().toLowerCase();

    if (normalizedBlockType === 'special') {
      return 'special';
    }

    if (normalizedBlockType === 'electric') {
      return 'electric';
    }

    if (normalizedBlockType === 'damaged') {
      return 'damaged';
    }

    if (normalizedBlockType === 'virtual') {
      return 'virtual';
    }

    return 'normal';
  }

  getSlotClass(
    position: ContainerPositionResponse | null | undefined,
    block?: BlockResponse | null
  ): string {
    if (!position) {
      return 'empty';
    }

    const blockType = block?.blockType?.trim().toLowerCase();

    if (blockType === 'special') {
      return 'occupied special';
    }

    if (blockType === 'electric') {
      return 'occupied electric';
    }

    if (blockType === 'damaged') {
      return 'occupied damaged';
    }

    return 'occupied normal';
  }

  getSlotLabel(position: ContainerPositionResponse | null | undefined): string {
    if (!position) {
      return 'Trống';
    }

    const container = this.getContainerById(position.containerId);

    return this.getShortContainerNumber(container?.containerNumber || `#${position.containerId}`);
  }

  isVirtualBlock(block: BlockResponse | null | undefined): boolean {
    return block?.blockType?.trim().toLowerCase() === 'virtual';
  }

  isMoveSourceCell(blockId: number, bay: number, row: number, tier: number): boolean {
    if (!this.moveSourceSlot) {
      return false;
    }

    return (
      this.moveSourceSlot.block.id === blockId &&
      this.moveSourceSlot.bay === bay &&
      this.moveSourceSlot.row === row &&
      this.moveSourceSlot.tier === tier
    );
  }

  isMoveTargetCell(blockId: number, bay: number, row: number, tier: number): boolean {
    if (!this.moveTargetSlot) {
      return false;
    }

    return (
      this.moveTargetSlot.block.id === blockId &&
      this.moveTargetSlot.bay === bay &&
      this.moveTargetSlot.row === row &&
      this.moveTargetSlot.tier === tier
    );
  }

  getMoveSourceText(): string {
    if (!this.moveSourceSlot || !this.moveSourceSlot.container) {
      return 'Bấm vào ô có container để chọn nguồn';
    }

    return `${this.getShortContainerNumber(this.moveSourceSlot.container.containerNumber)} · ${this.getPositionDisplayText(this.moveSourceSlot)}`;
  }

  getMoveTargetText(): string {
    if (!this.moveTargetSlot) {
      return 'Sau đó bấm vào ô trống để chọn đích';
    }

    return this.getPositionDisplayText(this.moveTargetSlot);
  }

  getPositionDisplayText(slot: YardSlotSelection | null): string {
    if (!slot) {
      return 'Chưa chọn';
    }

    return `${this.getBlockDisplayName(slot.block)} · Bay ${slot.bay} · Row ${slot.row} · Tier ${slot.tier}`;
  }

  trackByBlockId(_index: number, block: BlockResponse): number {
    return block.id;
  }

  trackByNumber(_index: number, value: number): number {
    return value;
  }

  private getDefaultSelectedBlockId(): number | null {
    const firstPhysicalBlock = this.getPhysicalBlocks()[0];

    if (firstPhysicalBlock) {
      return firstPhysicalBlock.id;
    }

    return this.blocks[0]?.id ?? null;
  }

  private normalizeSelectedTier(): void {
    const selectedBlock = this.getSelectedBlock();

    if (!selectedBlock || this.isVirtualBlock(selectedBlock)) {
      this.selectedTier = 1;
      return;
    }

    const maxTier = selectedBlock.maxTier ?? 1;

    if (this.selectedTier < 1) {
      this.selectedTier = 1;
      return;
    }

    if (this.selectedTier > maxTier) {
      this.selectedTier = maxTier;
    }
  }

  private createNumberRange(maxValue: number): number[] {
    if (maxValue <= 0) {
      return [];
    }

    return Array.from({ length: maxValue }, (_value, index) => index + 1);
  }

  private isSameSlot(first: YardSlotSelection | null, second: YardSlotSelection | null): boolean {
    if (!first || !second) {
      return false;
    }

    return (
      first.block.id === second.block.id &&
      first.bay === second.bay &&
      first.row === second.row &&
      first.tier === second.tier
    );
  }

  private getCurrentDateTimeInputValue(): string {
    const now = new Date();
    const timezoneOffset = now.getTimezoneOffset() * 60000;
    const localDate = new Date(now.getTime() - timezoneOffset);

    return localDate.toISOString().slice(0, 16);
  }

  private getApiErrorMessage(error: unknown, fallbackMessage: string): string {
    const responseBody = (error as any)?.error;

    if (typeof responseBody === 'string') {
      return responseBody;
    }

    if (responseBody?.Message) {
      return responseBody.Message;
    }

    if (responseBody?.message) {
      return responseBody.message;
    }

    if (responseBody?.Title) {
      return responseBody.Title;
    }

    if (responseBody?.title) {
      return responseBody.title;
    }

    if (responseBody?.Errors) {
      if (Array.isArray(responseBody.Errors)) {
        return responseBody.Errors
          .map((item: any) => item?.Message || item?.message || item)
          .filter((itemMessage: string) => !!itemMessage)
          .join(' ');
      }

      const firstKey = Object.keys(responseBody.Errors)[0];

      if (firstKey) {
        const firstError = responseBody.Errors[firstKey];

        if (Array.isArray(firstError)) {
          return firstError.join(' ');
        }

        return String(firstError);
      }
    }

    if (responseBody?.errors) {
      if (Array.isArray(responseBody.errors)) {
        return responseBody.errors
          .map((item: any) => item?.Message || item?.message || item)
          .filter((itemMessage: string) => !!itemMessage)
          .join(' ');
      }

      const firstKey = Object.keys(responseBody.errors)[0];

      if (firstKey) {
        const firstError = responseBody.errors[firstKey];

        if (Array.isArray(firstError)) {
          return firstError.join(' ');
        }

        return String(firstError);
      }
    }

    return (error as any)?.message || fallbackMessage;
  }
}
