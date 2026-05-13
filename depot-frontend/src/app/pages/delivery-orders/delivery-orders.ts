import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { DeliveryOrderResponse } from '../../models/delivery-order.model';
import { DeliveryOrderService } from '../../services/delivery-order.service';

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

  isLoading = false;
  errorMessage = '';

  searchKeyword = '';
  selectedStatus = 'All Status';
  selectedExpiryStatus = 'All Expiry';

  statusOptions: string[] = ['All Status'];

  constructor(
    private readonly deliveryOrderService: DeliveryOrderService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDeliveryOrders();
  }

  loadDeliveryOrders(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.deliveryOrderService.getDeliveryOrders(1, 100).subscribe({
      next: (response) => {
        this.deliveryOrders = response.items ?? [];

        this.buildStatusOptions();
        this.applyFilters();

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
        (this.selectedExpiryStatus === 'Valid' && !this.isDeliveryOrderExpired(order)) ||
        (this.selectedExpiryStatus === 'Expired' && this.isDeliveryOrderExpired(order));

      return matchesKeyword && matchesStatus && matchesExpiry;
    });

    if (
      this.selectedDeliveryOrder &&
      !this.filteredDeliveryOrders.some((order) => order.id === this.selectedDeliveryOrder?.id)
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
    return this.normalizeText(status) === '-' ? 'Chưa cập nhật' : this.normalizeText(status);
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
}
