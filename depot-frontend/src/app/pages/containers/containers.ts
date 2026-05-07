import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import {
  ContainerResponse,
  CreateContainerRequest,
  UpdateContainerRequest,
} from '../../models/container.model';

import { ContainerTypeResponse } from '../../models/container-type.model';
import { LineOperatorResponse } from '../../models/line-operator.model';

import { ContainerService } from '../../services/container.service';
import { ContainerTypeService } from '../../services/container-type.service';
import { LineOperatorService } from '../../services/line-operator.service';

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

        this.isSubmittingContainer = false;
        this.isContainerModalOpen = false;

        const updatedId = this.editingContainerId;
        this.isEditMode = false;
        this.editingContainerId = null;
        this.resetContainerForm();

        this.loadContainers();

        if (updatedId) {
          const selectedAfterUpdate = this.allContainers.find(
            (container) => container.id === updatedId
          );

          if (selectedAfterUpdate) {
            this.selectedContainer = selectedAfterUpdate;
          }
        }

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
    this.applyFilters();

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
}
