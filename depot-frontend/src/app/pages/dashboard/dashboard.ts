import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';

import { BlockResponse } from '../../models/block.model';
import { ContainerPositionResponse } from '../../models/container-position.model';
import { ContainerTransactionResponse } from '../../models/container-transaction.model';
import { ContainerResponse } from '../../models/container.model';
import { DeliveryOrderResponse } from '../../models/delivery-order.model';
import {
  ContainerThroughputReportResponse,
  ContainerYardInventoryReportResponse,
  ReportDateRequest,
} from '../../models/report.model';

import { BlockService } from '../../services/block.service';
import { ContainerPositionService } from '../../services/container-position.service';
import { ContainerTransactionService } from '../../services/container-transaction.service';
import { ContainerService } from '../../services/container.service';
import { DeliveryOrderService } from '../../services/delivery-order.service';
import { ReportService } from '../../services/report.service';

interface DashboardAlert {
  type: 'warning' | 'danger' | 'info' | 'success';
  title: string;
  description: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  containers: ContainerResponse[] = [];
  blocks: BlockResponse[] = [];
  positions: ContainerPositionResponse[] = [];
  recentTransactions: ContainerTransactionResponse[] = [];
  deliveryOrders: DeliveryOrderResponse[] = [];
  throughputReports: ContainerThroughputReportResponse[] = [];
  yardInventoryReports: ContainerYardInventoryReportResponse[] = [];

  isLoading = false;
  errorMessage = '';
  lastUpdatedAt: Date | null = null;

  private readonly pageSize = 100;

  constructor(
    private readonly containerService: ContainerService,
    private readonly blockService: BlockService,
    private readonly containerPositionService: ContainerPositionService,
    private readonly containerTransactionService: ContainerTransactionService,
    private readonly deliveryOrderService: DeliveryOrderService,
    private readonly reportService: ReportService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.changeDetectorRef.detectChanges();

    const todayRequest: ReportDateRequest = {
      date: this.getTodayDateInputValue(),
    };

    forkJoin({
      containers: this.containerService.getContainers(1, this.pageSize),
      blocks: this.blockService.getBlocks(1, this.pageSize),
      positions: this.containerPositionService.getContainerPositions(1, this.pageSize),
      transactions: this.containerTransactionService.getContainerTransactions(1, 20),
      deliveryOrders: this.deliveryOrderService.getDeliveryOrders(1, this.pageSize),
      throughputReports: this.reportService.getThroughputReport(todayRequest),
      yardInventoryReports: this.reportService.getYardInventoryReport(todayRequest),
    }).subscribe({
      next: (response) => {
        this.containers = response.containers.items ?? [];
        this.blocks = response.blocks.items ?? [];
        this.positions = response.positions.items ?? [];
        this.recentTransactions = this.sortTransactionsByTime(
          response.transactions.items ?? []
        ).slice(0, 6);
        this.deliveryOrders = response.deliveryOrders.items ?? [];
        this.throughputReports = response.throughputReports ?? [];
        this.yardInventoryReports = response.yardInventoryReports ?? [];

        this.lastUpdatedAt = new Date();
        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load dashboard failed:', error);

        this.errorMessage = this.getApiErrorMessage(
          error,
          'Không tải được Dashboard. Hãy kiểm tra backend Docker, proxy hoặc Network tab.'
        );

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  getTotalContainers(): number {
    return this.containers.length;
  }

  getInYardContainers(): number {
    return this.containers.filter(
      (container) => this.normalizeText(container.currentStatus) === 'inyard'
    ).length;
  }

  getOutYardContainers(): number {
    return this.containers.filter(
      (container) => this.normalizeText(container.currentStatus) === 'outyard'
    ).length;
  }

  getDamagedContainers(): number {
    return this.containers.filter(
      (container) => this.normalizeText(container.containerCondition) === 'damaged'
    ).length;
  }

  getInspectionContainers(): number {
    return this.containers.filter(
      (container) => this.normalizeText(container.containerCondition) === 'inspection'
    ).length;
  }

  getTodayImportCount(): number {
    return this.throughputReports.reduce((total, report) => total + report.importCount, 0);
  }

  getTodayExportCount(): number {
    return this.throughputReports.reduce((total, report) => total + report.exportCount, 0);
  }

  getTodayThroughputCount(): number {
    return this.getTodayImportCount() + this.getTodayExportCount();
  }

  getActiveDeliveryOrderCount(): number {
    return this.deliveryOrders.filter(
      (order) => this.normalizeText(order.orderStatus) === 'active'
    ).length;
  }

  getExpiringDeliveryOrderCount(): number {
    return this.deliveryOrders.filter((order) => this.isDeliveryOrderExpiringSoon(order)).length;
  }

  getOccupiedSlotCount(): number {
    return this.positions.length;
  }

  getTotalPhysicalSlotCount(): number {
    return this.blocks
      .filter((block) => block.blockType !== 'Virtual')
      .reduce((total, block) => {
        const bay = block.maxBay ?? 0;
        const row = block.maxRow ?? 0;
        const tier = block.maxTier ?? 0;

        return total + bay * row * tier;
      }, 0);
  }

  getYardOccupancyPercent(): number {
    const totalSlots = this.getTotalPhysicalSlotCount();

    if (totalSlots <= 0) {
      return 0;
    }

    return Math.min(Math.round((this.getOccupiedSlotCount() / totalSlots) * 100), 100);
  }

  getTopYardInventoryReports(): ContainerYardInventoryReportResponse[] {
    return [...this.yardInventoryReports]
      .sort((firstItem, secondItem) => secondItem.totalInYardCount - firstItem.totalInYardCount)
      .slice(0, 5);
  }

  getMaxYardInventoryCount(): number {
    const values = this.yardInventoryReports.map((item) => item.totalInYardCount);

    return Math.max(...values, 1);
  }

  getInventoryBarWidth(value: number): string {
    const maxValue = this.getMaxYardInventoryCount();

    if (!value || value <= 0) {
      return '0%';
    }

    const width = Math.round((value / maxValue) * 100);

    return `${Math.max(width, 8)}%`;
  }

  getDashboardAlerts(): DashboardAlert[] {
    const alerts: DashboardAlert[] = [];
    const occupancyPercent = this.getYardOccupancyPercent();
    const damagedCount = this.getDamagedContainers();
    const inspectionCount = this.getInspectionContainers();
    const expiringDoCount = this.getExpiringDeliveryOrderCount();

    if (occupancyPercent >= 80) {
      alerts.push({
        type: 'danger',
        title: `Tồn bãi đang cao: ${occupancyPercent}%`,
        description: 'Cần theo dõi sức chứa block và kế hoạch xuất bãi.',
      });
    }

    if (damagedCount > 0) {
      alerts.push({
        type: 'warning',
        title: `${damagedCount} container hư hỏng`,
        description: 'Cần đưa vào block Damaged hoặc xử lý trước khi xuất.',
      });
    }

    if (inspectionCount > 0) {
      alerts.push({
        type: 'info',
        title: `${inspectionCount} container chờ kiểm định`,
        description: 'Cần kiểm tra tình trạng trước khi đưa vào luồng xuất bãi.',
      });
    }

    if (expiringDoCount > 0) {
      alerts.push({
        type: 'warning',
        title: `${expiringDoCount} Delivery Order gần hết hạn`,
        description: 'Nên xử lý sớm để tránh bị backend tự chuyển Cancelled khi quá hạn.',
      });
    }

    if (alerts.length === 0) {
      alerts.push({
        type: 'success',
        title: 'Không có cảnh báo nổi bật',
        description: 'Các chỉ số vận hành chính đang trong trạng thái ổn định.',
      });
    }

    return alerts;
  }

  getContainerDisplayName(containerId: number): string {
    const container = this.containers.find((item) => item.id === containerId);

    return container?.containerNumber ?? `Container #${containerId}`;
  }

  getTransactionTypeDisplayName(type: string): string {
    switch (type) {
      case 'In':
        return 'Nhập bãi';
      case 'Out':
        return 'Xuất bãi';
      case 'Move':
        return 'Di dời';
      default:
        return type || 'Không rõ';
    }
  }

  getTransactionClass(type: string): string {
    return this.normalizeText(type) || 'unknown';
  }

  getTransactionPositionDisplay(transaction: ContainerTransactionResponse): string {
    if (transaction.transactionType === 'Out') {
      return this.formatPosition(
        transaction.fromBlockId,
        transaction.fromBay,
        transaction.fromRow,
        transaction.fromTier
      );
    }

    return this.formatPosition(
      transaction.toBlockId,
      transaction.toBay,
      transaction.toRow,
      transaction.toTier
    );
  }

  getLineOperatorDisplayName(
    code: string | null | undefined,
    name: string | null | undefined,
    id: number | null | undefined
  ): string {
    const lineCode = code?.trim();
    const lineName = name?.trim();

    if (lineCode && lineName) {
      return `${lineCode} - ${lineName}`;
    }

    if (lineCode) {
      return lineCode;
    }

    if (lineName) {
      return lineName;
    }

    if (id) {
      return `Line ${id}`;
    }

    return 'Chưa có hãng';
  }

  getLastUpdatedDisplay(): string {
    if (!this.lastUpdatedAt) {
      return 'Chưa tải dữ liệu';
    }

    return this.lastUpdatedAt.toLocaleTimeString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  getTodayDisplay(): string {
    const today = new Date();

    return today.toLocaleDateString('vi-VN');
  }

  private formatPosition(
    blockId: number | null,
    bay: number | null,
    row: number | null,
    tier: number | null
  ): string {
    if (!blockId || !bay || !row || !tier) {
      return '-';
    }

    const block = this.blocks.find((item) => item.id === blockId);
    const blockCode = block?.blockCode ?? `Block ${blockId}`;

    return `${blockCode} / Bay ${bay} / Row ${row} / Tier ${tier}`;
  }

  private sortTransactionsByTime(
    transactions: ContainerTransactionResponse[]
  ): ContainerTransactionResponse[] {
    return [...transactions].sort((firstTransaction, secondTransaction) => {
      const firstTime = this.getTransactionTimeValue(firstTransaction.transactionTime);
      const secondTime = this.getTransactionTimeValue(secondTransaction.transactionTime);

      return secondTime - firstTime;
    });
  }

  private getTransactionTimeValue(transactionTime: string | null): number {
    if (!transactionTime) {
      return 0;
    }

    const timeValue = new Date(transactionTime).getTime();

    return Number.isNaN(timeValue) ? 0 : timeValue;
  }

  private isDeliveryOrderExpiringSoon(order: DeliveryOrderResponse): boolean {
    if (this.normalizeText(order.orderStatus) !== 'active') {
      return false;
    }

    const expiryDate = new Date(order.expiryDate);

    if (Number.isNaN(expiryDate.getTime())) {
      return false;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const nextSevenDays = new Date(today);
    nextSevenDays.setDate(today.getDate() + 7);

    return expiryDate >= today && expiryDate <= nextSevenDays;
  }

  private normalizeText(value: string | null | undefined): string {
    return value?.trim().toLowerCase() ?? '';
  }

  private getApiErrorMessage(error: any, fallbackMessage: string): string {
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

    return fallbackMessage;
  }

  private getTodayDateInputValue(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = `${today.getMonth() + 1}`.padStart(2, '0');
    const day = `${today.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
