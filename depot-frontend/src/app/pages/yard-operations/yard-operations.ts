import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ContainerResponse } from '../../models/container.model';
import { ImportContainerRequest } from '../../models/container-transaction.model';

import { ContainerService } from '../../services/container.service';
import { ContainerTransactionService } from '../../services/container-transaction.service';

type YardOperationTab = 'import' | 'export' | 'move' | 'history';

interface YardOperationHistoryItem {
  id: string;
  containerNumber: string;
  transactionType: 'In' | 'Out' | 'Move';
  fromPosition: string;
  toPosition: string;
  vehicleNumber: string;
  transactionTime: string;
  note: string;
}

@Component({
  selector: 'app-yard-operations',
  imports: [CommonModule, FormsModule],
  templateUrl: './yard-operations.html',
  styleUrl: './yard-operations.scss',
})
export class YardOperations implements OnInit {
  activeTab: YardOperationTab = 'import';

  containers: ContainerResponse[] = [];
  isLoadingContainers = false;

  isImportSubmitting = false;
  importSuccessMessage = '';
  importErrorMessage = '';

  importForm = {
    containerId: null as number | null,
    toBlockId: null as number | null,
    toBay: null as number | null,
    toRow: null as number | null,
    toTier: null as number | null,
    vehicleNumber: '',
    transactionTime: this.getCurrentDateTimeInputValue(),
    note: '',
  };

  exportForm = {
    containerNumber: '',
    deliveryOrderNumber: '',
    vehicleNumber: '',
    transactionTime: this.getCurrentDateTimeInputValue(),
    note: '',
  };

  moveForm = {
    containerNumber: '',
    currentPositionId: '',
    currentBlock: '',
    currentBay: null as number | null,
    currentRow: null as number | null,
    currentTier: null as number | null,
    newBlock: '',
    newBay: null as number | null,
    newRow: null as number | null,
    newTier: null as number | null,
    positionTime: this.getCurrentDateTimeInputValue(),
    note: '',
  };

  historyItems: YardOperationHistoryItem[] = [
    {
      id: 'TRX-001',
      containerNumber: 'CMAU1234564',
      transactionType: 'In',
      fromPosition: '-',
      toPosition: 'A01 / Bay 1 / Row 2 / Tier 1',
      vehicleNumber: '51C-12345',
      transactionTime: '2026-05-06 08:30',
      note: 'Container nhập bãi',
    },
    {
      id: 'TRX-002',
      containerNumber: 'TEMU1234565',
      transactionType: 'Move',
      fromPosition: 'A01 / Bay 1 / Row 1 / Tier 1',
      toPosition: 'A01 / Bay 3 / Row 2 / Tier 1',
      vehicleNumber: 'RTG-02',
      transactionTime: '2026-05-06 10:15',
      note: 'Di dời nội bộ',
    },
    {
      id: 'TRX-003',
      containerNumber: 'MSCU1234567',
      transactionType: 'Out',
      fromPosition: 'B02 / Bay 2 / Row 1 / Tier 1',
      toPosition: '-',
      vehicleNumber: '51D-67890',
      transactionTime: '2026-05-06 14:20',
      note: 'Xuất container theo DO',
    },
  ];

  constructor(
    private readonly containerService: ContainerService,
    private readonly containerTransactionService: ContainerTransactionService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadContainers();
  }

  loadContainers(): void {
    this.isLoadingContainers = true;
    this.changeDetectorRef.detectChanges();

    this.containerService.getContainers(0, 100).subscribe({
      next: (response) => {
        this.containers = response.items ?? [];
        this.isLoadingContainers = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load containers for yard operations failed:', error);

        this.importErrorMessage =
          'Không tải được danh sách container. Hãy kiểm tra backend Docker hoặc proxy.';

        this.isLoadingContainers = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  setActiveTab(tab: YardOperationTab): void {
    this.activeTab = tab;
    this.changeDetectorRef.detectChanges();
  }

  submitImport(): void {
    this.importSuccessMessage = '';
    this.importErrorMessage = '';

    const validationMessage = this.validateImportForm();

    if (validationMessage) {
      this.importErrorMessage = validationMessage;
      this.changeDetectorRef.detectChanges();
      return;
    }

    const selectedContainerBeforeImport = this.getSelectedImportContainer();

    const request: ImportContainerRequest = {
      containerId: this.importForm.containerId as number,
      toBlockId: this.importForm.toBlockId as number,
      toBay: this.importForm.toBay as number,
      toRow: this.importForm.toRow as number,
      toTier: this.importForm.toTier as number,
      vehicleNumber: this.importForm.vehicleNumber.trim() || null,
      transactionTime: this.importForm.transactionTime || null,
      note: this.importForm.note.trim() || null,
    };

    this.isImportSubmitting = true;
    this.changeDetectorRef.detectChanges();

    this.containerTransactionService.importContainer(request).subscribe({
      next: (transactionId) => {
        console.log('Import container success. Transaction id:', transactionId);

        this.importSuccessMessage = `Nhập bãi thành công. Mã giao dịch: ${transactionId}.`;
        this.importErrorMessage = '';

        this.historyItems = [
          {
            id: `TRX-${transactionId}`,
            containerNumber:
              selectedContainerBeforeImport?.containerNumber ?? `ID ${request.containerId}`,
            transactionType: 'In',
            fromPosition: '-',
            toPosition: `Block ${request.toBlockId} / Bay ${request.toBay} / Row ${request.toRow} / Tier ${request.toTier}`,
            vehicleNumber: request.vehicleNumber ?? '-',
            transactionTime: this.formatDateTimeForDisplay(request.transactionTime),
            note: request.note ?? 'Container nhập bãi',
          },
          ...this.historyItems,
        ];

        this.resetImportForm();

        this.isImportSubmitting = false;
        this.changeDetectorRef.detectChanges();

        this.loadContainers();
      },
      error: (error) => {
        console.error('Import container failed:', error);

        this.importErrorMessage = this.getApiErrorMessage(
          error,
          'Không nhập được container. Hãy kiểm tra container, vị trí bãi hoặc quy tắc nghiệp vụ.'
        );

        this.importSuccessMessage = '';
        this.isImportSubmitting = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  validateImportForm(): string {
    if (!this.importForm.containerId) {
      return 'Vui lòng chọn container cần nhập bãi.';
    }

    if (this.isEmptyNumber(this.importForm.toBlockId)) {
      return 'Vui lòng nhập Block.';
    }

    if ((this.importForm.toBlockId as number) <= 0) {
      return 'Block phải lớn hơn 0.';
    }

    if (this.isEmptyNumber(this.importForm.toBay)) {
      return 'Vui lòng nhập Bay.';
    }

    if ((this.importForm.toBay as number) <= 0) {
      return 'Bay phải lớn hơn 0.';
    }

    if (this.isEmptyNumber(this.importForm.toRow)) {
      return 'Vui lòng nhập Row.';
    }

    if ((this.importForm.toRow as number) <= 0) {
      return 'Row phải lớn hơn 0.';
    }

    if (this.isEmptyNumber(this.importForm.toTier)) {
      return 'Vui lòng nhập Tier.';
    }

    if ((this.importForm.toTier as number) <= 0) {
      return 'Tier phải lớn hơn 0.';
    }

    return '';
  }

  resetImportForm(): void {
    this.importForm = {
      containerId: null,
      toBlockId: null,
      toBay: null,
      toRow: null,
      toTier: null,
      vehicleNumber: '',
      transactionTime: this.getCurrentDateTimeInputValue(),
      note: '',
    };
  }

  submitExport(): void {
    console.log('Export form:', this.exportForm);
  }

  submitMove(): void {
    console.log('Move form:', this.moveForm);
  }

  getSelectedImportContainer(): ContainerResponse | undefined {
    if (!this.importForm.containerId) {
      return undefined;
    }

    return this.containers.find((container) => container.id === this.importForm.containerId);
  }

  getApiErrorMessage(error: any, fallbackMessage: string): string {
    const responseBody = error?.error;

    if (typeof responseBody === 'string') {
      return this.normalizeApiErrorMessage(responseBody);
    }

    if (responseBody?.Message) {
      return this.normalizeApiErrorMessage(responseBody.Message);
    }

    if (responseBody?.message) {
      return this.normalizeApiErrorMessage(responseBody.message);
    }

    if (responseBody?.Title) {
      return this.normalizeApiErrorMessage(responseBody.Title);
    }

    if (responseBody?.title) {
      return this.normalizeApiErrorMessage(responseBody.title);
    }

    if (responseBody?.Errors) {
      if (Array.isArray(responseBody.Errors)) {
        const message = responseBody.Errors
          .map((item: any) => item?.Message || item?.message || item)
          .filter((itemMessage: string) => !!itemMessage)
          .join(' ');

        return this.normalizeApiErrorMessage(message);
      }

      const firstKey = Object.keys(responseBody.Errors)[0];

      if (firstKey) {
        const firstError = responseBody.Errors[firstKey];

        if (Array.isArray(firstError)) {
          return this.normalizeApiErrorMessage(firstError.join(' '));
        }

        return this.normalizeApiErrorMessage(String(firstError));
      }
    }

    if (responseBody?.errors) {
      if (Array.isArray(responseBody.errors)) {
        const message = responseBody.errors
          .map((item: any) => item?.Message || item?.message || item)
          .filter((itemMessage: string) => !!itemMessage)
          .join(' ');

        return this.normalizeApiErrorMessage(message);
      }

      const firstKey = Object.keys(responseBody.errors)[0];

      if (firstKey) {
        const firstError = responseBody.errors[firstKey];

        if (Array.isArray(firstError)) {
          return this.normalizeApiErrorMessage(firstError.join(' '));
        }

        return this.normalizeApiErrorMessage(String(firstError));
      }
    }

    return fallbackMessage;
  }

  getTransactionClass(type: string): string {
    const normalizedType = type.toLowerCase();

    if (normalizedType === 'in') {
      return 'in';
    }

    if (normalizedType === 'out') {
      return 'out';
    }

    if (normalizedType === 'move') {
      return 'move';
    }

    return '';
  }

  formatDateTimeForDisplay(dateTimeValue: string | null): string {
    if (!dateTimeValue) {
      return '-';
    }

    return dateTimeValue.replace('T', ' ');
  }

  private isEmptyNumber(value: number | null): boolean {
    return value === null || value === undefined;
  }

  private normalizeApiErrorMessage(message: string): string {
    return message
      .replace(/ToBlockId/g, 'Block')
      .replace(/toBlockId/g, 'Block')
      .replace(/ToBay/g, 'Bay')
      .replace(/toBay/g, 'Bay')
      .replace(/ToRow/g, 'Row')
      .replace(/toRow/g, 'Row')
      .replace(/ToTier/g, 'Tier')
      .replace(/toTier/g, 'Tier')
      .replace(/containerId/g, 'container')
      .replace(/ContainerId/g, 'container');
  }

  private getCurrentDateTimeInputValue(): string {
    const now = new Date();

    const year = now.getFullYear();
    const month = `${now.getMonth() + 1}`.padStart(2, '0');
    const day = `${now.getDate()}`.padStart(2, '0');
    const hour = `${now.getHours()}`.padStart(2, '0');
    const minute = `${now.getMinutes()}`.padStart(2, '0');

    return `${year}-${month}-${day}T${hour}:${minute}`;
  }
}
