import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ContainerResponse } from '../../models/container.model';
import { ContainerService } from '../../services/container.service';

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

  constructor(
    private readonly containerService: ContainerService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadContainers();
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
        this.selectedContainer = this.containers.length > 0 ? this.containers[0] : null;

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

  buildFilterOptions(): void {
    const statuses = this.allContainers
      .map((container) => container.currentStatus)
      .filter((status) => !!status);

    const lineOperators = this.allContainers
      .map((container) => container.lineOperatorCode || container.lineOperatorName)
      .filter((line) => !!line);

    const conditions = this.allContainers
      .map((container) => container.containerCondition)
      .filter((condition) => !!condition);

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

  getStatusClass(status: string): string {
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

  getConditionClass(condition: string): string {
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
