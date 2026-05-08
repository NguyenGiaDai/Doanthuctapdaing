import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ContainerResponse } from '../../models/container.model';
import {
  ContainerTransactionResponse,
  ExportContainerRequest,
  ImportContainerRequest,
} from '../../models/container-transaction.model';
import { DeliveryOrderResponse } from '../../models/delivery-order.model';

import { ContainerService } from '../../services/container.service';
import { ContainerTransactionService } from '../../services/container-transaction.service';
import { DeliveryOrderService } from '../../services/delivery-order.service';

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
  deliveryOrders: DeliveryOrderResponse[] = [];

  isLoadingContainers = false;
  isLoadingDeliveryOrders = false;

  isLoadingHistory = false;
  historyErrorMessage = '';

  isImportSubmitting = false;
  importSuccessMessage = '';
  importErrorMessage = '';

  isExportSubmitting = false;
  exportSuccessMessage = '';
  exportErrorMessage = '';

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
    containerId: null as number | null,
    deliveryOrderId: null as number | null,
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

  allHistoryItems: YardOperationHistoryItem[] = [];
  historyItems: YardOperationHistoryItem[] = [];
  historySearchKeyword = '';

  constructor(
    private readonly containerService: ContainerService,
    private readonly containerTransactionService: ContainerTransactionService,
    private readonly deliveryOrderService: DeliveryOrderService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadContainers();
    this.loadDeliveryOrders();
  }

  loadContainers(): void {
    this.isLoadingContainers = true;
    this.changeDetectorRef.detectChanges();

    this.containerService.getContainers(0, 100).subscribe({
      next: (response) => {
        this.containers = response.items ?? [];
        this.isLoadingContainers = false;

        this.loadTransactionHistory();

        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load containers for yard operations failed:', error);

        this.importErrorMessage =
          'Không tải được danh sách container. Hãy kiểm tra backend Docker hoặc proxy.';

        this.isLoadingContainers = false;

        this.loadTransactionHistory();

        this.changeDetectorRef.detectChanges();
      },
    });
  }

  loadDeliveryOrders(): void {
    this.isLoadingDeliveryOrders = true;
    this.changeDetectorRef.detectChanges();

    this.deliveryOrderService.getDeliveryOrders(1, 100).subscribe({
      next: (response) => {
        this.deliveryOrders = response.items ?? [];
        this.isLoadingDeliveryOrders = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load delivery orders failed:', error);

        this.exportErrorMessage =
          'Không tải được danh sách Delivery Order. Hãy kiểm tra backend Docker, proxy hoặc API DeliveryOrder.';

        this.deliveryOrders = [];
        this.isLoadingDeliveryOrders = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  loadTransactionHistory(): void {
    this.isLoadingHistory = true;
    this.historyErrorMessage = '';
    this.changeDetectorRef.detectChanges();

    this.containerTransactionService.getContainerTransactions(1, 50).subscribe({
      next: (response) => {
        const transactions = response.items ?? [];

        this.allHistoryItems = transactions
          .sort((firstTransaction, secondTransaction) => {
            const firstTime = this.getTransactionTimeValue(firstTransaction.transactionTime);
            const secondTime = this.getTransactionTimeValue(secondTransaction.transactionTime);

            return secondTime - firstTime;
          })
          .map((transaction) => this.mapTransactionToHistoryItem(transaction));

        this.applyHistorySearch();
        this.isLoadingHistory = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load transaction history failed:', error);

        this.historyErrorMessage =
          'Không tải được lịch sử container. Hãy kiểm tra backend Docker, proxy hoặc API ContainerTransaction.';

        this.allHistoryItems = [];
        this.historyItems = [];
        this.isLoadingHistory = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  setActiveTab(tab: YardOperationTab): void {
    this.activeTab = tab;

    if (tab === 'history') {
      this.loadTransactionHistory();
    }

    if (tab === 'export' && this.deliveryOrders.length === 0) {
      this.loadDeliveryOrders();
    }

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

  applyHistorySearch(): void {
    const keyword = this.historySearchKeyword.trim().toLowerCase();

    if (!keyword) {
      this.historyItems = [...this.allHistoryItems];
      this.changeDetectorRef.detectChanges();
      return;
    }

    this.historyItems = this.allHistoryItems.filter((item) => {
      return (
        item.id.toLowerCase().includes(keyword) ||
        item.containerNumber.toLowerCase().includes(keyword) ||
        item.transactionType.toLowerCase().includes(keyword) ||
        item.fromPosition.toLowerCase().includes(keyword) ||
        item.toPosition.toLowerCase().includes(keyword) ||
        item.vehicleNumber.toLowerCase().includes(keyword) ||
        item.transactionTime.toLowerCase().includes(keyword) ||
        item.note.toLowerCase().includes(keyword)
      );
    });

    this.changeDetectorRef.detectChanges();
  }

  clearHistorySearch(): void {
    this.historySearchKeyword = '';
    this.applyHistorySearch();
  }

  submitExport(): void {
    this.exportSuccessMessage = '';
    this.exportErrorMessage = '';

    const validationMessage = this.validateExportForm();

    if (validationMessage) {
      this.exportErrorMessage = validationMessage;
      this.changeDetectorRef.detectChanges();
      return;
    }

    const request: ExportContainerRequest = {
      containerId: this.exportForm.containerId as number,
      deliveryOrderId: this.exportForm.deliveryOrderId as number,
      vehicleNumber: this.exportForm.vehicleNumber.trim() || null,
      transactionTime: this.exportForm.transactionTime || null,
      note: this.exportForm.note.trim() || null,
    };

    this.isExportSubmitting = true;
    this.changeDetectorRef.detectChanges();

    this.containerTransactionService.exportContainer(request).subscribe({
      next: (transactionId) => {
        console.log('Export container success. Transaction id:', transactionId);

        this.exportSuccessMessage = `Xuất bãi thành công. Mã giao dịch: ${transactionId}.`;
        this.exportErrorMessage = '';

        this.resetExportForm();

        this.isExportSubmitting = false;
        this.changeDetectorRef.detectChanges();

        this.loadContainers();
        this.loadTransactionHistory();
      },
      error: (error) => {
        console.error('Export container failed:', error);

        this.exportErrorMessage = this.getApiErrorMessage(
          error,
          'Không xuất được container. Hãy kiểm tra container, Delivery Order hoặc quy tắc nghiệp vụ.'
        );

        this.exportSuccessMessage = '';
        this.isExportSubmitting = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  validateExportForm(): string {
    if (!this.exportForm.containerId) {
      return 'Vui lòng chọn container cần xuất bãi.';
    }

    const selectedContainer = this.getSelectedExportContainer();

    if (selectedContainer?.currentStatus !== 'InYard') {
      return 'Container phải đang ở trong bãi mới được xuất.';
    }

    if (this.isEmptyNumber(this.exportForm.deliveryOrderId)) {
      return 'Vui lòng chọn Delivery Order.';
    }

    const selectedDeliveryOrder = this.getSelectedDeliveryOrder();

    if (!selectedDeliveryOrder) {
      return 'Delivery Order không tồn tại trong danh sách.';
    }

    if (this.isDeliveryOrderExpired(selectedDeliveryOrder)) {
      return 'Delivery Order đã hết hạn, không thể xuất container.';
    }

    if (
      selectedContainer &&
      selectedDeliveryOrder.lineOperatorId !== selectedContainer.lineOperatorId
    ) {
      return 'Delivery Order không cùng hãng khai thác với container.';
    }

    if (
      selectedContainer &&
      selectedDeliveryOrder.containerTypeId !== selectedContainer.containerTypeId
    ) {
      return 'Delivery Order không đúng loại container.';
    }

    return '';
  }

  resetExportForm(): void {
    this.exportForm = {
      containerId: null,
      deliveryOrderId: null,
      vehicleNumber: '',
      transactionTime: this.getCurrentDateTimeInputValue(),
      note: '',
    };
  }

  onExportContainerChanged(): void {
    this.exportForm.deliveryOrderId = null;
    this.exportSuccessMessage = '';
    this.exportErrorMessage = '';
    this.changeDetectorRef.detectChanges();
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

  getSelectedExportContainer(): ContainerResponse | undefined {
    if (!this.exportForm.containerId) {
      return undefined;
    }

    return this.containers.find((container) => container.id === this.exportForm.containerId);
  }

  getSelectedDeliveryOrder(): DeliveryOrderResponse | undefined {
    if (!this.exportForm.deliveryOrderId) {
      return undefined;
    }

    return this.deliveryOrders.find((order) => order.id === this.exportForm.deliveryOrderId);
  }

  getFilteredDeliveryOrders(): DeliveryOrderResponse[] {
    const selectedContainer = this.getSelectedExportContainer();

    if (!selectedContainer) {
      return this.deliveryOrders.filter((order) => !this.isDeliveryOrderExpired(order));
    }

    return this.deliveryOrders.filter((order) => {
      return (
        order.lineOperatorId === selectedContainer.lineOperatorId &&
        order.containerTypeId === selectedContainer.containerTypeId &&
        !this.isDeliveryOrderExpired(order)
      );
    });
  }

  getDeliveryOrderDisplayText(order: DeliveryOrderResponse): string {
    const expiryDate = this.formatDateForDisplay(order.expiryDate);
    const lineOperator = order.lineOperatorName || `Line ${order.lineOperatorId}`;
    const containerType = order.containerTypeName || `Type ${order.containerTypeId}`;

    return `${order.doNumber} - ${lineOperator} - ${containerType} - Hạn: ${expiryDate}`;
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

  private mapTransactionToHistoryItem(
    transaction: ContainerTransactionResponse
  ): YardOperationHistoryItem {
    return {
      id: `TRX-${transaction.id}`,
      containerNumber: this.getContainerNumberById(transaction.containerId),
      transactionType: this.normalizeTransactionType(transaction.transactionType),
      fromPosition: this.formatPosition(
        transaction.fromBlockId,
        transaction.fromBay,
        transaction.fromRow,
        transaction.fromTier
      ),
      toPosition: this.formatPosition(
        transaction.toBlockId,
        transaction.toBay,
        transaction.toRow,
        transaction.toTier
      ),
      vehicleNumber: transaction.vehicleNumber || '-',
      transactionTime: this.formatDateTimeForDisplay(transaction.transactionTime),
      note: transaction.note || '-',
    };
  }

  private getContainerNumberById(containerId: number): string {
    const container = this.containers.find((item) => item.id === containerId);

    return container?.containerNumber ?? `ID ${containerId}`;
  }

  private normalizeTransactionType(transactionType: string): 'In' | 'Out' | 'Move' {
    const normalizedType = transactionType.toLowerCase();

    if (normalizedType === 'out') {
      return 'Out';
    }

    if (normalizedType === 'move') {
      return 'Move';
    }

    return 'In';
  }

  private formatPosition(
    blockId: number | null,
    bay: number | null,
    row: number | null,
    tier: number | null
  ): string {
    if (!blockId && !bay && !row && !tier) {
      return '-';
    }

    const blockText = blockId ? `Block ${blockId}` : 'Block -';
    const bayText = bay ? `Bay ${bay}` : 'Bay -';
    const rowText = row ? `Row ${row}` : 'Row -';
    const tierText = tier ? `Tier ${tier}` : 'Tier -';

    return `${blockText} / ${bayText} / ${rowText} / ${tierText}`;
  }

  private getTransactionTimeValue(transactionTime: string | null): number {
    if (!transactionTime) {
      return 0;
    }

    const timeValue = new Date(transactionTime).getTime();

    if (Number.isNaN(timeValue)) {
      return 0;
    }

    return timeValue;
  }

  private isEmptyNumber(value: number | null): boolean {
    return value === null || value === undefined;
  }

  private isDeliveryOrderExpired(order: DeliveryOrderResponse): boolean {
    if (!order.expiryDate) {
      return false;
    }

    const expiryDate = new Date(order.expiryDate);
    const today = new Date();

    expiryDate.setHours(23, 59, 59, 999);
    today.setHours(0, 0, 0, 0);

    if (Number.isNaN(expiryDate.getTime())) {
      return false;
    }

    return expiryDate < today;
  }

  private formatDateForDisplay(dateValue: string | null): string {
    if (!dateValue) {
      return '-';
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
      return dateValue;
    }

    const day = `${date.getDate()}`.padStart(2, '0');
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
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
      .replace(/ContainerId/g, 'container')
      .replace(/deliveryOrderId/g, 'Delivery Order')
      .replace(/DeliveryOrderId/g, 'Delivery Order');
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
