import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ContainerResponse } from '../../models/container.model';
import { ContainerService } from '../../services/container.service';

@Component({
  selector: 'app-containers',
  imports: [CommonModule],
  templateUrl: './containers.html',
  styleUrl: './containers.scss',
})
export class Containers implements OnInit {
  containers: ContainerResponse[] = [];
  selectedContainer: ContainerResponse | null = null;

  isLoading = false;
  errorMessage = '';

  totalCount = 0;
  currentPage = 1;
  pageSize = 10;

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

        this.containers = response.items ?? [];
        this.totalCount = response.totalCount ?? this.containers.length;
        this.currentPage = response.currentPage ?? 1;
        this.selectedContainer = this.containers.length > 0 ? this.containers[0] : null;

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();

        console.log('Containers loaded:', {
          isLoading: this.isLoading,
          totalCount: this.totalCount,
          containers: this.containers,
          selectedContainer: this.selectedContainer,
        });
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
