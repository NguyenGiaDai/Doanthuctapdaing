import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import {
  ContainerResponse,
  CreateContainerRequest,
  UpdateContainerRequest,
} from '../../models/container.model';

import { ContainerTransactionResponse } from '../../models/container-transaction.model';
import { ContainerTypeResponse } from '../../models/container-type.model';
import { LineOperatorResponse } from '../../models/line-operator.model';

import { ContainerService } from '../../services/container.service';
import { ContainerTransactionService } from '../../services/container-transaction.service';
import { ContainerTypeService } from '../../services/container-type.service';
import { LineOperatorService } from '../../services/line-operator.service';

interface ContainerHistoryPosition {
  title: string;
  status: string;
  block: string;
  bay: string;
  row: string;
  tier: string;
  description: string;
}

@Component({
  selector: 'app-containers',
  imports: [CommonModule, FormsModule],
  templateUrl: './containers.html',
  styleUrl: './containers.scss',
})
export class Containers implements OnInit {
  allContainers: ContainerResponse[] = [];
  containers: ContainerResponse[] = [];
  selectedContainer: ContainerResponse | null = null;

  isLoading = false;
  errorMessage = '';

  totalCount = 0;
  currentPage = 1;
  pageSize = 10;

  searchKeyword = '';
  selectedStatus = 'All Status';
  selectedLineOperator = 'All Lines';
  selectedCondition = 'All Conditions';

  statusOptions: string[] = ['All Status'];
  lineOperatorOptions: string[] = ['All Lines'];
  conditionOptions: string[] = ['All Conditions'];

  containerTypes: ContainerTypeResponse[] = [];
  lineOperators: LineOperatorResponse[] = [];

  isDropdownLoading = false;
  dropdownErrorMessage = '';

  isContainerModalOpen = false;
  isEditMode = false;
  editingContainerId: number | null = null;
  isSubmittingContainer = false;
  containerFormErrorMessage = '';

  isHistoryModalOpen = false;
  isHistoryLoading = false;
  historyErrorMessage = '';
  historyContainer: ContainerResponse | null = null;
  previousPosition: ContainerHistoryPosition | null = null;
  currentPosition: ContainerHistoryPosition | null = null;
  latestHistoryTransaction: ContainerTransactionResponse | null = null;

  containerForm = {
    containerNumber: '',
    containerTypeId: null as number | null,
    lineOperatorId: null as number | null,
    dateOfManufacture: '',
    containerOwner: '',
    containerCondition: 'Normal',
    containerClassification: 'A',
    currentStatus: 'OutYard',
  };

  constructor(
    private readonly containerService: ContainerService,
    private readonly containerTransactionService: ContainerTransactionService,
    private readonly containerTypeService: ContainerTypeService,
    private readonly lineOperatorService: LineOperatorService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadContainers();
    this.loadContainerDropdowns();
  }

  loadContainers(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.changeDetectorRef.detectChanges();

    this.containerService.getContainers(0, this.pageSize).subscribe({
      next: (response) => {
        console.log('Container API response:', response);

        this.allContainers = response.items ?? [];
        this.containers = [...this.allContainers];

        this.totalCount = this.containers.length;
        this.currentPage = response.currentPage ?? 1;

        this.buildFilterOptions();
        this.applyFiltersAfterReload();

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load containers failed:', error);

        this.errorMessage =
          'Không tải được danh sách container. Hãy kiểm tra backend Docker, proxy hoặc Network tab.';

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  loadContainerDropdowns(): void {
    this.isDropdownLoading = true;
    this.dropdownErrorMessage = '';
    this.changeDetectorRef.detectChanges();

    forkJoin({
      containerTypes: this.containerTypeService.getContainerTypes(),
      lineOperators: this.lineOperatorService.getLineOperators(),
    }).subscribe({
      next: (response) => {
        console.log('Container type dropdown response:', response.containerTypes);
        console.log('Line operator dropdown response:', response.lineOperators);

        this.containerTypes = response.containerTypes.items ?? [];
        this.lineOperators = response.lineOperators.items ?? [];

        this.isDropdownLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load container dropdowns failed:', error);

        this.dropdownErrorMessage =
          'Không tải được dữ liệu loại container hoặc hãng khai thác.';

        this.isDropdownLoading = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  openAddContainerModal(): void {
    this.isEditMode = false;
    this.editingContainerId = null;
    this.containerFormErrorMessage = '';
    this.resetContainerForm();
    this.isContainerModalOpen = true;

    if (this.containerTypes.length === 0 || this.lineOperators.length === 0) {
      this.loadContainerDropdowns();
    }
  }

  openEditContainerModal(container: ContainerResponse): void {
    this.isEditMode = true;
    this.editingContainerId = container.id;
    this.containerFormErrorMessage = '';

    this.containerForm = {
      containerNumber: container.containerNumber ?? '',
      containerTypeId: container.containerTypeId || null,
      lineOperatorId: container.lineOperatorId || null,
      dateOfManufacture: this.toDateInputValue(container.dateOfManufacture),
      containerOwner: container.containerOwner ?? '',
      containerCondition: container.containerCondition || 'Normal',
      containerClassification: container.containerClassification || 'A',
      currentStatus: container.currentStatus || 'OutYard',
    };

    this.isContainerModalOpen = true;

    if (this.containerTypes.length === 0 || this.lineOperators.length === 0) {
      this.loadContainerDropdowns();
    }
  }

  closeContainerModal(): void {
    if (this.isSubmittingContainer) {
      return;
    }

    this.isContainerModalOpen = false;
    this.containerFormErrorMessage = '';
    this.isEditMode = false;
    this.editingContainerId = null;
  }

  openContainerHistoryModal(container: ContainerResponse): void {
    this.historyContainer = container;
    this.isHistoryModalOpen = true;
    this.isHistoryLoading = true;
    this.historyErrorMessage = '';
    this.previousPosition = null;
    this.currentPosition = null;
    this.latestHistoryTransaction = null;
    this.changeDetectorRef.detectChanges();

    this.containerTransactionService.getContainerTransactions(1, 100).subscribe({
      next: (response) => {
        const containerTransactions = (response.items ?? [])
          .filter((transaction) => transaction.containerId === container.id)
          .sort((firstTransaction, secondTransaction) => {
            const firstTime = this.getTransactionTimeValue(firstTransaction);
            const secondTime = this.getTransactionTimeValue(secondTransaction);

            return secondTime - firstTime;
          });

        if (containerTransactions.length === 0) {
          this.historyErrorMessage =
            'Container này chưa có lịch sử nhập, xuất hoặc di dời trong bãi.';
          this.isHistoryLoading = false;
          this.changeDetectorRef.detectChanges();
          return;
        }

        const latestTransaction = containerTransactions[0];

        this.latestHistoryTransaction = latestTransaction;
        this.previousPosition = this.buildPreviousPosition(latestTransaction);
        this.currentPosition = this.buildCurrentPosition(latestTransaction);

        this.isHistoryLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load selected container history failed:', error);

        this.historyErrorMessage =
          'Không tải được lịch sử container. Hãy kiểm tra backend Docker, proxy hoặc API ContainerTransaction.';

        this.isHistoryLoading = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  closeContainerHistoryModal(): void {
    if (this.isHistoryLoading) {
      return;
    }

    this.isHistoryModalOpen = false;
    this.historyErrorMessage = '';
    this.historyContainer = null;
    this.previousPosition = null;
    this.currentPosition = null;
    this.latestHistoryTransaction = null;
  }

  submitContainerForm(): void {
    this.containerFormErrorMessage = '';

    const validationMessage = this.validateContainerForm();

    if (validationMessage) {
      this.containerFormErrorMessage = validationMessage;
      this.changeDetectorRef.detectChanges();
      return;
    }

    if (this.isEditMode) {
      this.updateContainer();
      return;
    }

    this.createContainer();
  }

  createContainer(): void {
    const request: CreateContainerRequest = {
      containerNumber: this.containerForm.containerNumber.trim().toUpperCase(),
      containerTypeId: this.containerForm.containerTypeId as number,
      lineOperatorId: this.containerForm.lineOperatorId as number,
      dateOfManufacture: this.containerForm.dateOfManufacture || null,
      containerOwner: this.containerForm.containerOwner.trim(),
      containerCondition: this.containerForm.containerCondition,
      containerClassification: this.containerForm.containerClassification || null,
      currentStatus: this.containerForm.currentStatus || null,
    };

    this.isSubmittingContainer = true;
    this.changeDetectorRef.detectChanges();

    this.containerService.createContainer(request).subscribe({
      next: (createdContainer) => {
        console.log('Create container response:', createdContainer);

        this.isSubmittingContainer = false;
        this.isContainerModalOpen = false;
        this.resetContainerForm();

        this.loadContainers();
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Create container failed:', error);

        this.containerFormErrorMessage = this.getApiErrorMessage(
          error,
          'Không thêm được container. Hãy kiểm tra dữ liệu nhập hoặc backend.'
        );

        this.isSubmittingContainer = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  updateContainer(): void {
    if (!this.editingContainerId) {
      this.containerFormErrorMessage = 'Không xác định được container cần cập nhật.';
      return;
    }

    const request: UpdateContainerRequest = {
      id: this.editingContainerId,
      containerNumber: this.containerForm.containerNumber.trim().toUpperCase(),
      containerTypeId: this.containerForm.containerTypeId as number,
      lineOperatorId: this.containerForm.lineOperatorId as number,
      dateOfManufacture: this.containerForm.dateOfManufacture || null,
      containerOwner: this.containerForm.containerOwner.trim(),
      containerCondition: this.containerForm.containerCondition,
      containerClassification: this.containerForm.containerClassification || null,
      currentStatus: this.containerForm.currentStatus || null,
    };

    this.isSubmittingContainer = true;
    this.changeDetectorRef.detectChanges();

    this.containerService.updateContainer(request).subscribe({
      next: (updatedContainer) => {
        console.log('Update container response:', updatedContainer);

        const updatedId = this.editingContainerId;

        this.isSubmittingContainer = false;
        this.isContainerModalOpen = false;
        this.isEditMode = false;
        this.editingContainerId = null;
        this.resetContainerForm();

        if (updatedId && this.selectedContainer?.id === updatedId) {
          this.selectedContainer = {
            ...this.selectedContainer,
            ...request,
          };
        }

        this.loadContainers();
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Update container failed:', error);

        this.containerFormErrorMessage = this.getApiErrorMessage(
          error,
          'Không cập nhật được container. Hãy kiểm tra dữ liệu nhập hoặc backend.'
        );

        this.isSubmittingContainer = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  validateContainerForm(): string {
    if (!this.containerForm.containerNumber.trim()) {
      return 'Vui lòng nhập số container.';
    }

    if (!this.containerForm.containerTypeId) {
      return 'Vui lòng chọn loại container.';
    }

    if (!this.containerForm.lineOperatorId) {
      return 'Vui lòng chọn hãng khai thác.';
    }

    if (!this.containerForm.containerOwner.trim()) {
      return 'Vui lòng nhập chủ sở hữu container.';
    }

    if (!this.containerForm.containerCondition.trim()) {
      return 'Vui lòng chọn tình trạng container.';
    }

    return '';
  }

  resetContainerForm(): void {
    this.containerForm = {
      containerNumber: '',
      containerTypeId: null,
      lineOperatorId: null,
      dateOfManufacture: '',
      containerOwner: '',
      containerCondition: 'Normal',
      containerClassification: 'A',
      currentStatus: 'OutYard',
    };

    this.containerFormErrorMessage = '';
  }

  getApiErrorMessage(error: any, fallbackMessage: string): string {
    const responseBody = error?.error;

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
          .filter((message: string) => !!message)
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
          .filter((message: string) => !!message)
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

    return fallbackMessage;
  }

  buildFilterOptions(): void {
    const statuses = this.allContainers
      .map((container) => container.currentStatus)
      .filter((status): status is string => !!status);

    const lineOperators = this.allContainers
      .map((container) => container.lineOperatorCode || container.lineOperatorName)
      .filter((line): line is string => !!line);

    const conditions = this.allContainers
      .map((container) => container.containerCondition)
      .filter((condition): condition is string => !!condition);

    this.statusOptions = ['All Status', ...new Set(statuses)];
    this.lineOperatorOptions = ['All Lines', ...new Set(lineOperators)];
    this.conditionOptions = ['All Conditions', ...new Set(conditions)];
  }

  applyFilters(): void {
    const keyword = this.searchKeyword.trim().toLowerCase();

    this.containers = this.allContainers.filter((container) => {
      const matchesKeyword =
        !keyword ||
        container.containerNumber?.toLowerCase().includes(keyword) ||
        container.containerTypeCode?.toLowerCase().includes(keyword) ||
        container.containerTypeName?.toLowerCase().includes(keyword) ||
        container.isoCode?.toLowerCase().includes(keyword) ||
        container.lineOperatorCode?.toLowerCase().includes(keyword) ||
        container.lineOperatorName?.toLowerCase().includes(keyword) ||
        container.containerOwner?.toLowerCase().includes(keyword);

      const matchesStatus =
        this.selectedStatus === 'All Status' ||
        container.currentStatus === this.selectedStatus;

      const currentLine = container.lineOperatorCode || container.lineOperatorName || '';
      const matchesLineOperator =
        this.selectedLineOperator === 'All Lines' ||
        currentLine === this.selectedLineOperator;

      const matchesCondition =
        this.selectedCondition === 'All Conditions' ||
        container.containerCondition === this.selectedCondition;

      return matchesKeyword && matchesStatus && matchesLineOperator && matchesCondition;
    });

    this.totalCount = this.containers.length;

    if (this.containers.length === 0) {
      this.selectedContainer = null;
      return;
    }

    const selectedStillExists = this.selectedContainer
      ? this.containers.some((container) => container.id === this.selectedContainer?.id)
      : false;

    if (!selectedStillExists) {
      this.selectedContainer = this.containers[0];
    }
  }

  applyFiltersAfterReload(): void {
    const selectedContainerId = this.selectedContainer?.id;

    this.applyFilters();

    if (selectedContainerId) {
      const reloadedSelectedContainer = this.containers.find(
        (container) => container.id === selectedContainerId
      );

      if (reloadedSelectedContainer) {
        this.selectedContainer = reloadedSelectedContainer;
        return;
      }
    }

    if (!this.selectedContainer && this.containers.length > 0) {
      this.selectedContainer = this.containers[0];
    }
  }

  clearFilters(): void {
    this.searchKeyword = '';
    this.selectedStatus = 'All Status';
    this.selectedLineOperator = 'All Lines';
    this.selectedCondition = 'All Conditions';

    this.applyFilters();
  }

  selectContainer(container: ContainerResponse): void {
    this.selectedContainer = container;
  }

  toDateInputValue(dateValue: string | null | undefined): string {
    if (!dateValue) {
      return '';
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
      return dateValue.slice(0, 10);
    }

    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  formatWeight(weight: number | null | undefined): string {
    if (weight === null || weight === undefined) {
      return '-';
    }

    return new Intl.NumberFormat('en-US').format(weight);
  }

  formatDate(dateValue: string | null | undefined): string {
    if (!dateValue) {
      return '-';
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
      return dateValue;
    }

    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  formatHistoryDate(dateValue: string | null | undefined): string {
    if (!dateValue) {
      return '-';
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
      return dateValue.replace('T', ' ');
    }

    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    const hour = `${date.getHours()}`.padStart(2, '0');
    const minute = `${date.getMinutes()}`.padStart(2, '0');

    return `${year}-${month}-${day} ${hour}:${minute}`;
  }

  getStatusClass(status: string | null): string {
    const normalizedStatus = status?.toLowerCase();

    if (normalizedStatus === 'inyard') {
      return 'in-yard';
    }

    if (normalizedStatus === 'outyard') {
      return 'out-yard';
    }

    if (normalizedStatus === 'reserved') {
      return 'reserved';
    }

    return '';
  }

  getConditionClass(condition: string | null): string {
    const normalizedCondition = condition?.toLowerCase();

    if (normalizedCondition === 'normal' || normalizedCondition === 'good') {
      return 'normal';
    }

    if (normalizedCondition === 'damaged') {
      return 'damaged';
    }

    if (normalizedCondition === 'inspection') {
      return 'inspection';
    }

    return '';
  }

  private buildPreviousPosition(
    transaction: ContainerTransactionResponse
  ): ContainerHistoryPosition {
    const transactionType = transaction.transactionType?.toLowerCase();

    if (transactionType === 'in') {
      return {
        title: 'Vị trí trước đó',
        status: 'Ngoài bãi',
        block: '-',
        bay: '-',
        row: '-',
        tier: '-',
        description: 'Container chưa nằm trong bãi trước giao dịch nhập bãi.',
      };
    }

    if (transactionType === 'out') {
      return this.buildPositionCard(
        'Vị trí trước đó',
        'Trong bãi',
        transaction.fromBlockId,
        transaction.fromBay,
        transaction.fromRow,
        transaction.fromTier,
        'Vị trí container trước khi xuất khỏi bãi.'
      );
    }

    return this.buildPositionCard(
      'Vị trí trước đó',
      'Trong bãi',
      transaction.fromBlockId,
      transaction.fromBay,
      transaction.fromRow,
      transaction.fromTier,
      'Vị trí container trước lần di dời gần nhất.'
    );
  }

  private buildCurrentPosition(
    transaction: ContainerTransactionResponse
  ): ContainerHistoryPosition {
    const transactionType = transaction.transactionType?.toLowerCase();

    if (transactionType === 'out') {
      return {
        title: 'Vị trí hiện tại',
        status: 'Ngoài bãi',
        block: '-',
        bay: '-',
        row: '-',
        tier: '-',
        description: 'Container đã được xuất khỏi bãi.',
      };
    }

    if (transactionType === 'in') {
      return this.buildPositionCard(
        'Vị trí hiện tại',
        'Trong bãi',
        transaction.toBlockId,
        transaction.toBay,
        transaction.toRow,
        transaction.toTier,
        'Vị trí container sau khi nhập bãi.'
      );
    }

    return this.buildPositionCard(
      'Vị trí hiện tại',
      'Trong bãi',
      transaction.toBlockId,
      transaction.toBay,
      transaction.toRow,
      transaction.toTier,
      'Vị trí container sau lần di dời gần nhất.'
    );
  }

  private buildPositionCard(
    title: string,
    status: string,
    blockId: number | null,
    bay: number | null,
    row: number | null,
    tier: number | null,
    description: string
  ): ContainerHistoryPosition {
    return {
      title,
      status,
      block: blockId ? String(blockId) : '-',
      bay: bay ? String(bay) : '-',
      row: row ? String(row) : '-',
      tier: tier ? String(tier) : '-',
      description,
    };
  }

  private getTransactionTimeValue(transaction: ContainerTransactionResponse): number {
    if (transaction.transactionTime) {
      const timeValue = new Date(transaction.transactionTime).getTime();

      if (!Number.isNaN(timeValue)) {
        return timeValue;
      }
    }

    return transaction.id;
  }
}
