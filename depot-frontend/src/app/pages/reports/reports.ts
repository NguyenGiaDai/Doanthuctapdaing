import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import {
  ContainerThroughputReportResponse,
  ContainerYardInventoryReportResponse,
  ReportDateRequest,
} from '../../models/report.model';
import { ReportService } from '../../services/report.service';

@Component({
  selector: 'app-reports',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports implements OnInit {
  fromDate = this.getTodayDateInputValue();
  toDate = this.getTodayDateInputValue();

  throughputReports: ContainerThroughputReportResponse[] = [];
  yardInventoryReports: ContainerYardInventoryReportResponse[] = [];

  isLoadingThroughput = false;
  isLoadingInventory = false;

  throughputErrorMessage = '';
  inventoryErrorMessage = '';

  constructor(
    private readonly reportService: ReportService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    const validationMessage = this.validateDateRange();

    if (validationMessage) {
      this.throughputErrorMessage = validationMessage;
      this.inventoryErrorMessage = validationMessage;
      this.throughputReports = [];
      this.yardInventoryReports = [];
      this.changeDetectorRef.detectChanges();
      return;
    }

    this.loadThroughputReport();
    this.loadYardInventoryReport();
  }

  onDateRangeChanged(): void {
    this.throughputErrorMessage = '';
    this.inventoryErrorMessage = '';
    this.loadReports();
  }

  getTotalImportCount(): number {
    return this.throughputReports.reduce((total, item) => total + item.importCount, 0);
  }

  getTotalExportCount(): number {
    return this.throughputReports.reduce((total, item) => total + item.exportCount, 0);
  }

  getTotalThroughputCount(): number {
    return this.throughputReports.reduce(
      (total, item) => total + this.getThroughputTotal(item),
      0
    );
  }

  getTotalFrom0To10DaysCount(): number {
    return this.yardInventoryReports.reduce(
      (total, item) => total + item.from0To10DaysCount,
      0
    );
  }

  getTotalFrom10DaysOrMoreCount(): number {
    return this.yardInventoryReports.reduce(
      (total, item) => total + item.from10DaysOrMoreCount,
      0
    );
  }

  getTotalInYardCount(): number {
    return this.yardInventoryReports.reduce((total, item) => total + item.totalInYardCount, 0);
  }

  getReportRangeDisplay(): string {
    if (this.fromDate === this.toDate) {
      return this.formatDateForDisplay(this.fromDate);
    }

    return `${this.formatDateForDisplay(this.fromDate)} - ${this.formatDateForDisplay(this.toDate)}`;
  }

  getInventoryDateDisplay(): string {
    return this.formatDateForDisplay(this.toDate);
  }

  getLineOperatorDisplayName(
    lineOperatorCode: string | null | undefined,
    lineOperatorName: string | null | undefined,
    lineOperatorId: number | null | undefined
  ): string {
    const code = lineOperatorCode?.trim();
    const name = lineOperatorName?.trim();

    if (code && name) {
      return `${code} - ${name}`;
    }

    if (code) {
      return code;
    }

    if (name) {
      return name;
    }

    if (lineOperatorId) {
      return `Line ${lineOperatorId}`;
    }

    return 'Chưa có hãng khai thác';
  }

  getShortLineOperatorName(
    lineOperatorCode: string | null | undefined,
    lineOperatorName: string | null | undefined,
    lineOperatorId: number | null | undefined
  ): string {
    const code = lineOperatorCode?.trim();
    const name = lineOperatorName?.trim();

    if (code) {
      return code;
    }

    if (name) {
      return name;
    }

    if (lineOperatorId) {
      return `Line ${lineOperatorId}`;
    }

    return 'N/A';
  }

  getThroughputFlowClass(importCount: number, exportCount: number): string {
    if (importCount > exportCount) {
      return 'import-dominant';
    }

    if (exportCount > importCount) {
      return 'export-dominant';
    }

    return 'balanced';
  }

  getThroughputFlowDisplayName(importCount: number, exportCount: number): string {
    if (importCount > exportCount) {
      return 'Nhập nhiều hơn';
    }

    if (exportCount > importCount) {
      return 'Xuất nhiều hơn';
    }

    return 'Cân bằng';
  }

  getMaxThroughputValue(): number {
    const values = this.throughputReports.flatMap((item) => [
      item.importCount,
      item.exportCount,
    ]);

    return Math.max(...values, 1);
  }

  getMaxInventoryValue(): number {
    const values = this.yardInventoryReports.map((item) => item.totalInYardCount);

    return Math.max(...values, 1);
  }

  getBarWidth(value: number, maxValue: number): string {
    if (!value || value <= 0) {
      return '0%';
    }

    const width = Math.round((value / maxValue) * 100);

    return `${Math.max(width, 6)}%`;
  }

  getInventorySegmentWidth(value: number, total: number): string {
    if (!value || !total) {
      return '0%';
    }

    const width = Math.round((value / total) * 100);

    return `${Math.max(width, 6)}%`;
  }

  private loadThroughputReport(): void {
    const reportDates = this.getDatesInRange(this.fromDate, this.toDate);

    if (reportDates.length === 0) {
      this.throughputReports = [];
      this.throughputErrorMessage = 'Khoảng thời gian báo cáo không hợp lệ.';
      this.changeDetectorRef.detectChanges();
      return;
    }

    this.isLoadingThroughput = true;
    this.throughputErrorMessage = '';
    this.changeDetectorRef.detectChanges();

    const requests = reportDates.map((date) => {
      const request: ReportDateRequest = { date };

      return this.reportService.getThroughputReport(request);
    });

    forkJoin(requests).subscribe({
      next: (dailyReports) => {
        this.throughputReports = this.aggregateThroughputReports(dailyReports.flat());
        this.isLoadingThroughput = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load throughput report failed:', error);

        this.throughputReports = [];
        this.throughputErrorMessage = this.getApiErrorMessage(
          error,
          'Không tải được báo cáo sản lượng. Hãy kiểm tra backend Docker, proxy hoặc API report.'
        );
        this.isLoadingThroughput = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  private loadYardInventoryReport(): void {
    const request: ReportDateRequest = {
      date: this.toDate,
    };

    this.isLoadingInventory = true;
    this.inventoryErrorMessage = '';
    this.changeDetectorRef.detectChanges();

    this.reportService.getYardInventoryReport(request).subscribe({
      next: (response) => {
        this.yardInventoryReports = response ?? [];
        this.isLoadingInventory = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load yard inventory report failed:', error);

        this.yardInventoryReports = [];
        this.inventoryErrorMessage = this.getApiErrorMessage(
          error,
          'Không tải được báo cáo tồn bãi. Hãy kiểm tra backend Docker, proxy hoặc API report.'
        );
        this.isLoadingInventory = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  private aggregateThroughputReports(
    reports: ContainerThroughputReportResponse[]
  ): ContainerThroughputReportResponse[] {
    const reportMap = new Map<string, ContainerThroughputReportResponse>();

    for (const report of reports) {
      const key = this.getLineOperatorMapKey(report);
      const existingReport = reportMap.get(key);
      const throughputTotal = this.getThroughputTotal(report);

      if (!existingReport) {
        reportMap.set(key, {
          lineOperatorId: report.lineOperatorId,
          lineOperatorCode: report.lineOperatorCode,
          lineOperatorName: report.lineOperatorName,
          importCount: report.importCount,
          exportCount: report.exportCount,
          totalCount: throughputTotal,
        });

        continue;
      }

      existingReport.importCount += report.importCount;
      existingReport.exportCount += report.exportCount;
      existingReport.totalCount += throughputTotal;
    }

    return Array.from(reportMap.values()).sort((firstItem, secondItem) => {
      return secondItem.totalCount - firstItem.totalCount;
    });
  }

  private getThroughputTotal(report: ContainerThroughputReportResponse): number {
    return report.importCount + report.exportCount;
  }

  private getLineOperatorMapKey(report: ContainerThroughputReportResponse): string {
    if (report.lineOperatorId) {
      return `id-${report.lineOperatorId}`;
    }

    const code = report.lineOperatorCode?.trim().toLowerCase();

    if (code) {
      return `code-${code}`;
    }

    const name = report.lineOperatorName?.trim().toLowerCase();

    if (name) {
      return `name-${name}`;
    }

    return 'unknown-line-operator';
  }

  private validateDateRange(): string {
    if (!this.fromDate || !this.toDate) {
      return 'Vui lòng chọn đầy đủ Từ ngày và Đến ngày.';
    }

    const fromDateValue = new Date(this.fromDate).getTime();
    const toDateValue = new Date(this.toDate).getTime();

    if (Number.isNaN(fromDateValue) || Number.isNaN(toDateValue)) {
      return 'Ngày báo cáo không hợp lệ.';
    }

    if (fromDateValue > toDateValue) {
      return 'Từ ngày không được lớn hơn Đến ngày.';
    }

    return '';
  }

  private getDatesInRange(fromDate: string, toDate: string): string[] {
    const dates: string[] = [];

    const currentDate = new Date(fromDate);
    const endDate = new Date(toDate);

    if (Number.isNaN(currentDate.getTime()) || Number.isNaN(endDate.getTime())) {
      return dates;
    }

    while (currentDate <= endDate) {
      dates.push(this.formatDateForApi(currentDate));
      currentDate.setDate(currentDate.getDate() + 1);
    }

    return dates;
  }

  private formatDateForApi(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
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

  private formatDateForDisplay(dateValue: string): string {
    if (!dateValue) {
      return '-';
    }

    const dateParts = dateValue.split('-');

    if (dateParts.length === 3) {
      return `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;
    }

    return dateValue;
  }

  private getTodayDateInputValue(): string {
    const today = new Date();

    const year = today.getFullYear();
    const month = `${today.getMonth() + 1}`.padStart(2, '0');
    const day = `${today.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
