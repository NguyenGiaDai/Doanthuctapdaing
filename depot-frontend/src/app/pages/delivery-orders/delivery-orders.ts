import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import {
  CreateDeliveryOrderRequest,
  DeliveryOrderResponse,
} from '../../models/delivery-order.model';
import { CustomerResponse } from '../../models/customer.model';
import { LineOperatorResponse } from '../../models/line-operator.model';
import { ContainerTypeResponse } from '../../models/container-type.model';

import { DeliveryOrderService } from '../../services/delivery-order.service';
import { CustomerService } from '../../services/customer.service';
import { LineOperatorService } from '../../services/line-operator.service';
import { ContainerTypeService } from '../../services/container-type.service';

interface CreateDeliveryOrderForm {
  doNumber: string;
  customerId: number | null;
  lineOperatorId: number | null;
  containerTypeId: number | null;
  quantity: number | null;
  expiryDate: string;
  orderDate: string;
  vesselVoyage: string;
  orderStatus: string;
}

@Component({
  selector: 'app-delivery-orders',
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-orders.html',
  styleUrl: './delivery-orders.scss',
})
export class DeliveryOrders implements OnInit {
  deliveryOrders: DeliveryOrderResponse[] = [];
  filteredDeliveryOrders: DeliveryOrderResponse[] = [];
  selectedDeliveryOrder: DeliveryOrderResponse | null = null;

  customers: CustomerResponse[] = [];
  lineOperators: LineOperatorResponse[] = [];
  containerTypes: ContainerTypeResponse[] = [];

  isLoading = false;
  isLoadingMasterData = false;
  isCreateModalOpen = false;
  isSubmitting = false;

  errorMessage = '';
  masterDataErrorMessage = '';
  createErrorMessage = '';

  searchKeyword = '';
  selectedStatus = 'All Status';
  selectedExpiryStatus = 'All Expiry';

  statusOptions: string[] = ['All Status'];
  createStatusOptions: string[] = ['Active', 'Pending', 'Completed', 'Cancelled'];

  createForm: CreateDeliveryOrderForm = this.getDefaultCreateForm();

  constructor(
    private readonly deliveryOrderService: DeliveryOrderService,
    private readonly customerService: CustomerService,
    private readonly lineOperatorService: LineOperatorService,
    private readonly containerTypeService: ContainerTypeService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadMasterData();
    this.loadDeliveryOrders();
  }

  loadDeliveryOrders(selectedOrderId?: number): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.deliveryOrderService.getDeliveryOrders(1, 100).subscribe({
      next: (response) => {
        this.deliveryOrders = response.items ?? [];

        this.buildStatusOptions();
        this.applyFilters();

        if (selectedOrderId) {
          const createdOrder = this.filteredDeliveryOrders.find(
            (order) => order.id === selectedOrderId
          );

          if (createdOrder) {
            this.selectedDeliveryOrder = createdOrder;
          }
        }

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load delivery orders failed:', error);

        this.errorMessage =
          'Không tải được danh sách Delivery Order. Hãy kiểm tra backend Docker, proxy hoặc Network tab.';

        this.deliveryOrders = [];
        this.filteredDeliveryOrders = [];
        this.selectedDeliveryOrder = null;
        this.isLoading = false;

        this.changeDetectorRef.detectChanges();
      },
    });
  }

  loadMasterData(): void {
    this.isLoadingMasterData = true;
    this.masterDataErrorMessage = '';

    forkJoin({
      customers: this.customerService.getCustomers(1, 100),
      lineOperators: this.lineOperatorService.getLineOperators(1, 100),
      containerTypes: this.containerTypeService.getContainerTypes(1, 100),
    }).subscribe({
      next: ({ customers, lineOperators, containerTypes }) => {
        this.customers = customers.items ?? [];
        this.lineOperators = lineOperators.items ?? [];
        this.containerTypes = containerTypes.items ?? [];

        this.isLoadingMasterData = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load master data failed:', error);

        this.masterDataErrorMessage =
          'Không tải được dữ liệu khách hàng, hãng khai thác hoặc loại container.';

        this.customers = [];
        this.lineOperators = [];
        this.containerTypes = [];
        this.isLoadingMasterData = false;

        this.changeDetectorRef.detectChanges();
      },
    });
  }

  applyFilters(): void {
    const keyword = this.searchKeyword.trim().toLowerCase();

    this.filteredDeliveryOrders = this.deliveryOrders.filter((order) => {
      const matchesKeyword =
        !keyword ||
        [
          order.doNumber,
          order.customerName,
          order.lineOperatorName,
          order.containerTypeName,
          order.vesselVoyage,
          order.orderStatus,
        ]
          .filter(Boolean)
          .some((value) => String(value).toLowerCase().includes(keyword));

      const matchesStatus =
        this.selectedStatus === 'All Status' ||
        this.normalizeText(order.orderStatus) === this.selectedStatus;

      const matchesExpiry =
        this.selectedExpiryStatus === 'All Expiry' ||
        (this.selectedExpiryStatus === 'Valid' &&
          !this.isDeliveryOrderExpired(order)) ||
        (this.selectedExpiryStatus === 'Expired' &&
          this.isDeliveryOrderExpired(order));

      return matchesKeyword && matchesStatus && matchesExpiry;
    });

    if (
      this.selectedDeliveryOrder &&
      !this.filteredDeliveryOrders.some(
        (order) => order.id === this.selectedDeliveryOrder?.id
      )
    ) {
      this.selectedDeliveryOrder = this.filteredDeliveryOrders[0] ?? null;
    }

    if (!this.selectedDeliveryOrder && this.filteredDeliveryOrders.length > 0) {
      this.selectedDeliveryOrder = this.filteredDeliveryOrders[0];
    }
  }

  clearFilters(): void {
    this.searchKeyword = '';
    this.selectedStatus = 'All Status';
    this.selectedExpiryStatus = 'All Expiry';
    this.applyFilters();
  }

  selectDeliveryOrder(order: DeliveryOrderResponse): void {
    this.selectedDeliveryOrder = order;
  }

  openCreateModal(): void {
    this.createForm = this.getDefaultCreateForm();
    this.createErrorMessage = '';
    this.isCreateModalOpen = true;

    if (
      !this.isLoadingMasterData &&
      (this.customers.length === 0 ||
        this.lineOperators.length === 0 ||
        this.containerTypes.length === 0)
    ) {
      this.loadMasterData();
    }
  }

  closeCreateModal(): void {
    if (this.isSubmitting) {
      return;
    }

    this.isCreateModalOpen = false;
    this.createErrorMessage = '';
  }

  submitCreateDeliveryOrder(): void {
    this.createErrorMessage = '';

    const quantity = Number(this.createForm.quantity);
    const customerId = Number(this.createForm.customerId);
    const lineOperatorId = Number(this.createForm.lineOperatorId);
    const containerTypeId = Number(this.createForm.containerTypeId);

    if (!this.createForm.doNumber.trim()) {
      this.createErrorMessage = 'Vui lòng nhập số Delivery Order.';
      return;
    }

    if (!customerId || !lineOperatorId || !containerTypeId) {
      this.createErrorMessage =
        'Vui lòng chọn khách hàng, hãng khai thác và loại container.';
      return;
    }

    if (!quantity || quantity <= 0) {
      this.createErrorMessage = 'Số lượng container phải lớn hơn 0.';
      return;
    }

    if (!this.createForm.expiryDate) {
      this.createErrorMessage = 'Vui lòng chọn ngày hết hạn.';
      return;
    }

    const request: CreateDeliveryOrderRequest = {
      doNumber: this.createForm.doNumber.trim(),
      customerId,
      lineOperatorId,
      containerTypeId,
      quantity,
      expiryDate: this.toApiDate(this.createForm.expiryDate),
      orderDate: this.createForm.orderDate
        ? this.toApiDate(this.createForm.orderDate)
        : null,
      vesselVoyage: this.createForm.vesselVoyage.trim() || null,
      orderStatus: this.createForm.orderStatus.trim() || 'Active',
    };

    this.isSubmitting = true;

    this.deliveryOrderService.createDeliveryOrder(request).subscribe({
      next: (createdId) => {
        this.isSubmitting = false;
        this.isCreateModalOpen = false;
        this.createForm = this.getDefaultCreateForm();

        this.searchKeyword = '';
        this.selectedStatus = 'All Status';
        this.selectedExpiryStatus = 'All Expiry';

        this.loadDeliveryOrders(createdId);
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Create delivery order failed:', error);

        this.createErrorMessage = this.extractErrorMessage(
          error,
          'Không tạo được Delivery Order. Vui lòng kiểm tra dữ liệu nhập.'
        );

        this.isSubmitting = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  private buildStatusOptions(): void {
    const statuses = this.deliveryOrders
      .map((order) => this.normalizeText(order.orderStatus))
      .filter((status) => status !== '-');

    this.statusOptions = ['All Status', ...Array.from(new Set(statuses))];
  }

  isDeliveryOrderExpired(order: DeliveryOrderResponse): boolean {
    if (!order.expiryDate) {
      return false;
    }

    const expiryDate = new Date(order.expiryDate);

    if (Number.isNaN(expiryDate.getTime())) {
      return false;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    expiryDate.setHours(0, 0, 0, 0);

    return expiryDate < today;
  }

  getExpiryDisplayName(order: DeliveryOrderResponse): string {
    return this.isDeliveryOrderExpired(order) ? 'Hết hạn' : 'Còn hạn';
  }

  getExpiryClass(order: DeliveryOrderResponse): string {
    return this.isDeliveryOrderExpired(order) ? 'expired' : 'valid';
  }

  getOrderStatusDisplayName(status: string | null): string {
    return this.normalizeText(status) === '-'
      ? 'Chưa cập nhật'
      : this.normalizeText(status);
  }

  getOrderStatusClass(status: string | null): string {
    const normalizedStatus = this.normalizeText(status).toLowerCase();

    if (['active', 'open', 'valid'].includes(normalizedStatus)) {
      return 'active';
    }

    if (['completed', 'closed', 'done'].includes(normalizedStatus)) {
      return 'completed';
    }

    if (['cancelled', 'canceled'].includes(normalizedStatus)) {
      return 'cancelled';
    }

    if (normalizedStatus === 'expired') {
      return 'expired';
    }

    return 'pending';
  }

  getCustomerDisplayName(customer: CustomerResponse): string {
    return `${customer.customerCode} - ${customer.customerName}`;
  }

  getLineOperatorDisplayName(lineOperator: LineOperatorResponse): string {
    return `${lineOperator.lineOperatorCode} - ${lineOperator.lineOperatorName}`;
  }

  getContainerTypeDisplayName(containerType: ContainerTypeResponse): string {
    return `${containerType.containerTypeCode} - ${containerType.containerTypeName}`;
  }

  formatDate(value: string | null): string {
    if (!value) {
      return '-';
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return date.toLocaleDateString('vi-VN');
  }

  normalizeText(value: string | null): string {
    const text = value?.trim();

    return text && text.length > 0 ? text : '-';
  }

  private getDefaultCreateForm(): CreateDeliveryOrderForm {
    return {
      doNumber: '',
      customerId: null,
      lineOperatorId: null,
      containerTypeId: null,
      quantity: 1,
      expiryDate: '',
      orderDate: this.toDateInputValue(new Date()),
      vesselVoyage: '',
      orderStatus: 'Active',
    };
  }

  private toApiDate(value: string): string {
    return `${value}T00:00:00`;
  }

  private toDateInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private extractErrorMessage(error: any, fallbackMessage: string): string {
    return (
      error?.error?.Message ||
      error?.error?.message ||
      error?.error?.title ||
      error?.message ||
      fallbackMessage
    );
  }
}
