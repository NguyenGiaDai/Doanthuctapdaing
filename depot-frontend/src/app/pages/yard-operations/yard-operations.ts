import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

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
export class YardOperations {
  activeTab: YardOperationTab = 'import';

  importForm = {
    containerNumber: '',
    toBlock: '',
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

  setActiveTab(tab: YardOperationTab): void {
    this.activeTab = tab;
  }

  submitImport(): void {
    console.log('Import form:', this.importForm);
  }

  submitExport(): void {
    console.log('Export form:', this.exportForm);
  }

  submitMove(): void {
    console.log('Move form:', this.moveForm);
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
