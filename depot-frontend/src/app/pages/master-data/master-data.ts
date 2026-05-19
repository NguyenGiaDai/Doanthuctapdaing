import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { BlockResponse, CreateBlockRequest } from '../../models/block.model';
import {
  ContainerTypeResponse,
  CreateContainerTypeRequest,
} from '../../models/container-type.model';
import { CreateCustomerRequest, CustomerResponse } from '../../models/customer.model';
import { DepotResponse } from '../../models/depot.model';
import {
  CreateLineOperatorRequest,
  LineOperatorResponse,
} from '../../models/line-operator.model';

import { BlockService } from '../../services/block.service';
import { ContainerTypeService } from '../../services/container-type.service';
import { CustomerService } from '../../services/customer.service';
import { DepotService } from '../../services/depot.service';
import { LineOperatorService } from '../../services/line-operator.service';

type MasterDataTab = 'blocks' | 'containerTypes' | 'lineOperators' | 'customers';
type BlockType = 'Normal' | 'Special' | 'Electric' | 'Damaged' | 'Virtual';
type ContainerSizeOption = 20 | 40;

interface MasterDataTabOption {
  key: MasterDataTab;
  title: string;
  description: string;
}

interface CreateBlockForm {
  depotId: number | null;
  blockCode: string;
  blockName: string;
  blockType: BlockType;
  maxBay: number | null;
  maxRow: number | null;
  maxTier: number | null;
}

interface CreateContainerTypeForm {
  containerTypeCode: string;
  containerTypeName: string;
  isoCode: string;
  containerSize: ContainerSizeOption | null;
  maximumWeight: number | null;
  tareWeight: number | null;
}

interface CreateLineOperatorForm {
  lineOperatorCode: string;
  lineOperatorName: string;
}

interface CreateCustomerForm {
  customerCode: string;
  customerName: string;
  customerTaxCode: string;
  address: string;
}

@Component({
  selector: 'app-master-data',
  imports: [CommonModule, FormsModule],
  templateUrl: './master-data.html',
  styleUrl: './master-data.scss',
})
export class MasterData implements OnInit {
  readonly tabs: MasterDataTabOption[] = [
    {
      key: 'blocks',
      title: 'Block bãi',
      description: 'Khu vực lưu container theo loại block và sức chứa vật lý',
    },
    {
      key: 'containerTypes',
      title: 'Loại container',
      description: 'Danh mục loại, ISO code, kích thước và trọng lượng container',
    },
    {
      key: 'lineOperators',
      title: 'Hãng khai thác',
      description: 'Danh mục hãng tàu / line operator phục vụ nhập xuất container',
    },
    {
      key: 'customers',
      title: 'Khách hàng',
      description: 'Danh mục khách hàng sử dụng dịch vụ depot',
    },
  ];

  readonly blockTypes: BlockType[] = ['Normal', 'Special', 'Electric', 'Damaged', 'Virtual'];
  readonly containerSizeOptions: ContainerSizeOption[] = [20, 40];

  activeTab: MasterDataTab = 'blocks';
  searchKeyword = '';

  depots: DepotResponse[] = [];
  blocks: BlockResponse[] = [];
  containerTypes: ContainerTypeResponse[] = [];
  lineOperators: LineOperatorResponse[] = [];
  customers: CustomerResponse[] = [];

  selectedBlock: BlockResponse | null = null;
  selectedContainerType: ContainerTypeResponse | null = null;
  selectedLineOperator: LineOperatorResponse | null = null;
  selectedCustomer: CustomerResponse | null = null;

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  isCreateBlockModalOpen = false;
  isSubmittingBlock = false;
  createBlockErrorMessage = '';

  isCreateContainerTypeModalOpen = false;
  isSubmittingContainerType = false;
  createContainerTypeErrorMessage = '';

  isCreateLineOperatorModalOpen = false;
  isSubmittingLineOperator = false;
  createLineOperatorErrorMessage = '';

  isCreateCustomerModalOpen = false;
  isSubmittingCustomer = false;
  createCustomerErrorMessage = '';

  createBlockForm: CreateBlockForm = this.getDefaultCreateBlockForm();
  createContainerTypeForm: CreateContainerTypeForm = this.getDefaultCreateContainerTypeForm();
  createLineOperatorForm: CreateLineOperatorForm = this.getDefaultCreateLineOperatorForm();
  createCustomerForm: CreateCustomerForm = this.getDefaultCreateCustomerForm();

  constructor(
    private readonly blockService: BlockService,
    private readonly containerTypeService: ContainerTypeService,
    private readonly lineOperatorService: LineOperatorService,
    private readonly customerService: CustomerService,
    private readonly depotService: DepotService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadMasterData();
  }

  loadMasterData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      depots: this.depotService.getDepots(1, 100),
      blocks: this.blockService.getBlocks(1, 100),
      containerTypes: this.containerTypeService.getContainerTypes(1, 100),
      lineOperators: this.lineOperatorService.getLineOperators(1, 100),
      customers: this.customerService.getCustomers(1, 100),
    }).subscribe({
      next: (response) => {
        this.depots = response.depots.items ?? [];
        this.blocks = response.blocks.items ?? [];
        this.containerTypes = response.containerTypes.items ?? [];
        this.lineOperators = response.lineOperators.items ?? [];
        this.customers = response.customers.items ?? [];

        this.selectDefaultRows();

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Load master data failed:', error);

        this.errorMessage = this.getApiErrorMessage(
          error,
          'Không tải được dữ liệu Master Data. Hãy kiểm tra backend Docker, proxy hoặc Network tab.'
        );

        this.depots = [];
        this.blocks = [];
        this.containerTypes = [];
        this.lineOperators = [];
        this.customers = [];
        this.clearSelections();

        this.isLoading = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  setActiveTab(tab: MasterDataTab): void {
    this.activeTab = tab;
    this.searchKeyword = '';
    this.successMessage = '';
    this.errorMessage = '';
    this.selectDefaultForActiveTab();
  }

  openCreateModalForActiveTab(): void {
    this.successMessage = '';
    this.errorMessage = '';

    if (this.activeTab === 'blocks') {
      this.openCreateBlockModal();
      return;
    }

    if (this.activeTab === 'containerTypes') {
      this.openCreateContainerTypeModal();
      return;
    }

    if (this.activeTab === 'lineOperators') {
      this.openCreateLineOperatorModal();
      return;
    }

    if (this.activeTab === 'customers') {
      this.openCreateCustomerModal();
      return;
    }
  }

  openCreateBlockModal(): void {
    this.errorMessage = '';
    this.createBlockErrorMessage = '';
    this.createBlockForm = this.getDefaultCreateBlockForm();
    this.isCreateBlockModalOpen = true;
  }

  closeCreateBlockModal(): void {
    if (this.isSubmittingBlock) {
      return;
    }

    this.isCreateBlockModalOpen = false;
    this.createBlockErrorMessage = '';
  }

  onCreateBlockTypeChanged(): void {
    if (this.isCreateBlockVirtual()) {
      this.createBlockForm.maxBay = null;
      this.createBlockForm.maxRow = null;
      this.createBlockForm.maxTier = null;
    }
  }

  submitCreateBlock(): void {
    this.createBlockErrorMessage = this.validateCreateBlockForm();

    if (this.createBlockErrorMessage) {
      return;
    }

    const request = this.buildCreateBlockRequest();

    this.isSubmittingBlock = true;

    this.blockService.createBlock(request).subscribe({
      next: () => {
        this.isSubmittingBlock = false;
        this.isCreateBlockModalOpen = false;
        this.successMessage = `Đã thêm block ${request.blockCode}.`;
        this.activeTab = 'blocks';
        this.loadMasterData();
      },
      error: (error) => {
        console.error('Create block failed:', error);

        this.createBlockErrorMessage = this.getApiErrorMessage(
          error,
          'Không thêm được block. Hãy kiểm tra dữ liệu nhập hoặc backend response.'
        );

        this.isSubmittingBlock = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  openCreateContainerTypeModal(): void {
    this.errorMessage = '';
    this.createContainerTypeErrorMessage = '';
    this.createContainerTypeForm = this.getDefaultCreateContainerTypeForm();
    this.isCreateContainerTypeModalOpen = true;
  }

  closeCreateContainerTypeModal(): void {
    if (this.isSubmittingContainerType) {
      return;
    }

    this.isCreateContainerTypeModalOpen = false;
    this.createContainerTypeErrorMessage = '';
  }

  submitCreateContainerType(): void {
    this.createContainerTypeErrorMessage = this.validateCreateContainerTypeForm();

    if (this.createContainerTypeErrorMessage) {
      return;
    }

    const request = this.buildCreateContainerTypeRequest();

    this.isSubmittingContainerType = true;

    this.containerTypeService.createContainerType(request).subscribe({
      next: () => {
        this.isSubmittingContainerType = false;
        this.isCreateContainerTypeModalOpen = false;
        this.successMessage = `Đã thêm loại container ${request.containerTypeCode}.`;
        this.activeTab = 'containerTypes';
        this.loadMasterData();
      },
      error: (error) => {
        console.error('Create container type failed:', error);

        this.createContainerTypeErrorMessage = this.getApiErrorMessage(
          error,
          'Không thêm được loại container. Hãy kiểm tra dữ liệu nhập hoặc backend response.'
        );

        this.isSubmittingContainerType = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  openCreateLineOperatorModal(): void {
    this.errorMessage = '';
    this.createLineOperatorErrorMessage = '';
    this.createLineOperatorForm = this.getDefaultCreateLineOperatorForm();
    this.isCreateLineOperatorModalOpen = true;
  }

  closeCreateLineOperatorModal(): void {
    if (this.isSubmittingLineOperator) {
      return;
    }

    this.isCreateLineOperatorModalOpen = false;
    this.createLineOperatorErrorMessage = '';
  }

  submitCreateLineOperator(): void {
    this.createLineOperatorErrorMessage = this.validateCreateLineOperatorForm();

    if (this.createLineOperatorErrorMessage) {
      return;
    }

    const request = this.buildCreateLineOperatorRequest();

    this.isSubmittingLineOperator = true;

    this.lineOperatorService.createLineOperator(request).subscribe({
      next: () => {
        this.isSubmittingLineOperator = false;
        this.isCreateLineOperatorModalOpen = false;
        this.successMessage = `Đã khai báo hãng khai thác ${request.lineOperatorCode}.`;
        this.activeTab = 'lineOperators';
        this.loadMasterData();
      },
      error: (error) => {
        console.error('Create line operator failed:', error);

        this.createLineOperatorErrorMessage = this.getApiErrorMessage(
          error,
          'Không khai báo được hãng khai thác. Hãy kiểm tra dữ liệu nhập hoặc backend response.'
        );

        this.isSubmittingLineOperator = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  openCreateCustomerModal(): void {
    this.errorMessage = '';
    this.createCustomerErrorMessage = '';
    this.createCustomerForm = this.getDefaultCreateCustomerForm();
    this.isCreateCustomerModalOpen = true;
  }

  closeCreateCustomerModal(): void {
    if (this.isSubmittingCustomer) {
      return;
    }

    this.isCreateCustomerModalOpen = false;
    this.createCustomerErrorMessage = '';
  }

  submitCreateCustomer(): void {
    this.createCustomerErrorMessage = this.validateCreateCustomerForm();

    if (this.createCustomerErrorMessage) {
      return;
    }

    const request = this.buildCreateCustomerRequest();

    this.isSubmittingCustomer = true;

    this.customerService.createCustomer(request).subscribe({
      next: () => {
        this.isSubmittingCustomer = false;
        this.isCreateCustomerModalOpen = false;
        this.successMessage = `Đã khai báo khách hàng ${request.customerCode}.`;
        this.activeTab = 'customers';
        this.loadMasterData();
      },
      error: (error) => {
        console.error('Create customer failed:', error);

        this.createCustomerErrorMessage = this.getApiErrorMessage(
          error,
          'Không khai báo được khách hàng. Hãy kiểm tra dữ liệu nhập hoặc backend response.'
        );

        this.isSubmittingCustomer = false;
        this.changeDetectorRef.detectChanges();
      },
    });
  }

  selectBlock(block: BlockResponse): void {
    this.selectedBlock = block;
  }

  selectContainerType(containerType: ContainerTypeResponse): void {
    this.selectedContainerType = containerType;
  }

  selectLineOperator(lineOperator: LineOperatorResponse): void {
    this.selectedLineOperator = lineOperator;
  }

  selectCustomer(customer: CustomerResponse): void {
    this.selectedCustomer = customer;
  }

  getFilteredBlocks(): BlockResponse[] {
    const keyword = this.getNormalizedSearchKeyword();

    if (!keyword) {
      return this.blocks;
    }

    return this.blocks.filter((block) =>
      [
        block.blockCode,
        block.blockName,
        block.blockType,
        block.depotCode,
        block.depotName,
      ]
        .join(' ')
        .toLowerCase()
        .includes(keyword)
    );
  }

  getFilteredContainerTypes(): ContainerTypeResponse[] {
    const keyword = this.getNormalizedSearchKeyword();

    if (!keyword) {
      return this.containerTypes;
    }

    return this.containerTypes.filter((containerType) =>
      [
        containerType.containerTypeCode,
        containerType.containerTypeName,
        containerType.isoCode,
        containerType.containerSize,
      ]
        .join(' ')
        .toLowerCase()
        .includes(keyword)
    );
  }

  getFilteredLineOperators(): LineOperatorResponse[] {
    const keyword = this.getNormalizedSearchKeyword();

    if (!keyword) {
      return this.lineOperators;
    }

    return this.lineOperators.filter((lineOperator) =>
      [lineOperator.lineOperatorCode, lineOperator.lineOperatorName]
        .join(' ')
        .toLowerCase()
        .includes(keyword)
    );
  }

  getFilteredCustomers(): CustomerResponse[] {
    const keyword = this.getNormalizedSearchKeyword();

    if (!keyword) {
      return this.customers;
    }

    return this.customers.filter((customer) =>
      [
        customer.customerCode,
        customer.customerName,
        customer.customerTaxCode ?? '',
        customer.address ?? '',
      ]
        .join(' ')
        .toLowerCase()
        .includes(keyword)
    );
  }

  getActiveTabTitle(): string {
    return this.tabs.find((tab) => tab.key === this.activeTab)?.title ?? 'Master Data';
  }

  getActiveTabDescription(): string {
    return this.tabs.find((tab) => tab.key === this.activeTab)?.description ?? '';
  }

  getPrimaryActionLabel(): string {
    switch (this.activeTab) {
      case 'blocks':
        return 'Thêm block';
      case 'containerTypes':
        return 'Thêm loại container';
      case 'lineOperators':
        return 'Khai báo hãng khai thác';
      case 'customers':
        return 'Khai báo khách hàng';
    }
  }

  getActiveTabCount(): number {
    switch (this.activeTab) {
      case 'blocks':
        return this.blocks.length;
      case 'containerTypes':
        return this.containerTypes.length;
      case 'lineOperators':
        return this.lineOperators.length;
      case 'customers':
        return this.customers.length;
    }
  }

  getFilteredActiveTabCount(): number {
    switch (this.activeTab) {
      case 'blocks':
        return this.getFilteredBlocks().length;
      case 'containerTypes':
        return this.getFilteredContainerTypes().length;
      case 'lineOperators':
        return this.getFilteredLineOperators().length;
      case 'customers':
        return this.getFilteredCustomers().length;
    }
  }

  getTabCount(tab: MasterDataTab): number {
    switch (tab) {
      case 'blocks':
        return this.blocks.length;
      case 'containerTypes':
        return this.containerTypes.length;
      case 'lineOperators':
        return this.lineOperators.length;
      case 'customers':
        return this.customers.length;
    }
  }

  getPhysicalBlockCount(): number {
    return this.blocks.filter((block) => block.blockType !== 'Virtual').length;
  }

  getVirtualBlockCount(): number {
    return this.blocks.filter((block) => block.blockType === 'Virtual').length;
  }

  getBlockTypeDisplayName(blockType: string): string {
    switch (blockType) {
      case 'Normal':
        return 'Normal - hàng thường';
      case 'Special':
        return 'Special - hàng đặc biệt';
      case 'Electric':
        return 'Electric - hàng lạnh';
      case 'Damaged':
        return 'Damaged - hàng hư hỏng';
      case 'Virtual':
        return 'Virtual - block ảo';
      default:
        return blockType || 'Chưa xác định';
    }
  }

  getBlockTypeClass(blockType: string): string {
    return blockType?.toLowerCase() || 'unknown';
  }

  getBlockCapacityDisplay(block: BlockResponse): string {
    if (block.blockType === 'Virtual') {
      return 'Không áp dụng';
    }

    return `${block.maxBay ?? '-'} bay × ${block.maxRow ?? '-'} row × ${block.maxTier ?? '-'} tier`;
  }

  getWeightDisplay(value: number | null): string {
    if (value === null || value === undefined) {
      return '-';
    }

    return `${value.toLocaleString('vi-VN')} kg`;
  }

  isCreateBlockVirtual(): boolean {
    return this.createBlockForm.blockType === 'Virtual';
  }

  private getDefaultCreateBlockForm(): CreateBlockForm {
    return {
      depotId: null,
      blockCode: '',
      blockName: '',
      blockType: 'Normal',
      maxBay: null,
      maxRow: null,
      maxTier: null,
    };
  }

  private getDefaultCreateContainerTypeForm(): CreateContainerTypeForm {
    return {
      containerTypeCode: '',
      containerTypeName: '',
      isoCode: '',
      containerSize: 20,
      maximumWeight: null,
      tareWeight: null,
    };
  }

  private getDefaultCreateLineOperatorForm(): CreateLineOperatorForm {
    return {
      lineOperatorCode: '',
      lineOperatorName: '',
    };
  }

  private getDefaultCreateCustomerForm(): CreateCustomerForm {
    return {
      customerCode: '',
      customerName: '',
      customerTaxCode: '',
      address: '',
    };
  }

  private validateCreateBlockForm(): string {
    const blockCode = this.createBlockForm.blockCode.trim();
    const blockName = this.createBlockForm.blockName.trim();

    if (!this.createBlockForm.depotId) {
      return 'Vui lòng chọn depot cho block.';
    }

    if (!blockCode) {
      return 'Vui lòng nhập mã block.';
    }

    if (!blockName) {
      return 'Vui lòng nhập tên block.';
    }

    if (!this.createBlockForm.blockType) {
      return 'Vui lòng chọn loại block.';
    }

    if (!this.isCreateBlockVirtual()) {
      if (!this.isPositiveNumber(this.createBlockForm.maxBay)) {
        return 'Max Bay phải lớn hơn 0 cho block thật.';
      }

      if (!this.isPositiveNumber(this.createBlockForm.maxRow)) {
        return 'Max Row phải lớn hơn 0 cho block thật.';
      }

      if (!this.isPositiveNumber(this.createBlockForm.maxTier)) {
        return 'Max Tier phải lớn hơn 0 cho block thật.';
      }
    }

    return '';
  }

  private validateCreateContainerTypeForm(): string {
    const containerTypeCode = this.createContainerTypeForm.containerTypeCode.trim();
    const containerTypeName = this.createContainerTypeForm.containerTypeName.trim();
    const isoCode = this.createContainerTypeForm.isoCode.trim();

    if (!containerTypeCode) {
      return 'Vui lòng nhập mã loại container.';
    }

    if (!containerTypeName) {
      return 'Vui lòng nhập tên loại container.';
    }

    if (!isoCode) {
      return 'Vui lòng nhập ISO code.';
    }

    if (!this.isPositiveNumber(this.createContainerTypeForm.containerSize)) {
      return 'Vui lòng chọn kích thước container.';
    }

    if (
      this.createContainerTypeForm.maximumWeight !== null &&
      !this.isPositiveNumber(this.createContainerTypeForm.maximumWeight)
    ) {
      return 'Maximum Weight phải lớn hơn 0 nếu có nhập.';
    }

    if (
      this.createContainerTypeForm.tareWeight !== null &&
      !this.isPositiveNumber(this.createContainerTypeForm.tareWeight)
    ) {
      return 'Tare Weight phải lớn hơn 0 nếu có nhập.';
    }

    return '';
  }

  private validateCreateLineOperatorForm(): string {
    const lineOperatorCode = this.createLineOperatorForm.lineOperatorCode.trim();
    const lineOperatorName = this.createLineOperatorForm.lineOperatorName.trim();

    if (!lineOperatorCode) {
      return 'Vui lòng nhập mã hãng khai thác.';
    }

    if (!lineOperatorName) {
      return 'Vui lòng nhập tên hãng khai thác.';
    }

    return '';
  }

  private validateCreateCustomerForm(): string {
    const customerCode = this.createCustomerForm.customerCode.trim();
    const customerName = this.createCustomerForm.customerName.trim();

    if (!customerCode) {
      return 'Vui lòng nhập mã khách hàng.';
    }

    if (!customerName) {
      return 'Vui lòng nhập tên khách hàng.';
    }

    return '';
  }

  private buildCreateBlockRequest(): CreateBlockRequest {
    const isVirtualBlock = this.isCreateBlockVirtual();

    return {
      depotId: Number(this.createBlockForm.depotId),
      blockCode: this.createBlockForm.blockCode.trim().toUpperCase(),
      blockName: this.createBlockForm.blockName.trim(),
      blockType: this.createBlockForm.blockType,
      maxBay: isVirtualBlock ? null : Number(this.createBlockForm.maxBay),
      maxRow: isVirtualBlock ? null : Number(this.createBlockForm.maxRow),
      maxTier: isVirtualBlock ? null : Number(this.createBlockForm.maxTier),
    };
  }

  private buildCreateContainerTypeRequest(): CreateContainerTypeRequest {
    return {
      containerTypeCode: this.createContainerTypeForm.containerTypeCode.trim().toUpperCase(),
      containerTypeName: this.createContainerTypeForm.containerTypeName.trim(),
      isoCode: this.createContainerTypeForm.isoCode.trim().toUpperCase(),
      containerSize: Number(this.createContainerTypeForm.containerSize),
      maximumWeight: this.toNullableNumber(this.createContainerTypeForm.maximumWeight),
      tareWeight: this.toNullableNumber(this.createContainerTypeForm.tareWeight),
    };
  }

  private buildCreateLineOperatorRequest(): CreateLineOperatorRequest {
    return {
      lineOperatorCode: this.createLineOperatorForm.lineOperatorCode.trim().toUpperCase(),
      lineOperatorName: this.createLineOperatorForm.lineOperatorName.trim(),
    };
  }

  private buildCreateCustomerRequest(): CreateCustomerRequest {
    return {
      customerCode: this.createCustomerForm.customerCode.trim().toUpperCase(),
      customerName: this.createCustomerForm.customerName.trim(),
      customerTaxCode: this.toNullableText(this.createCustomerForm.customerTaxCode),
      address: this.toNullableText(this.createCustomerForm.address),
    };
  }

  private isPositiveNumber(value: number | null): boolean {
    return value !== null && Number(value) > 0;
  }

  private toNullableNumber(value: number | null): number | null {
    if (value === null || value === undefined) {
      return null;
    }

    const numberValue = Number(value);

    return Number.isNaN(numberValue) ? null : numberValue;
  }

  private toNullableText(value: string): string | null {
    const trimmedValue = value.trim();

    return trimmedValue ? trimmedValue : null;
  }

  private selectDefaultRows(): void {
    this.selectedBlock = this.blocks[0] ?? null;
    this.selectedContainerType = this.containerTypes[0] ?? null;
    this.selectedLineOperator = this.lineOperators[0] ?? null;
    this.selectedCustomer = this.customers[0] ?? null;
  }

  selectDefaultForActiveTab(): void {
    switch (this.activeTab) {
      case 'blocks':
        this.selectedBlock = this.getFilteredBlocks()[0] ?? null;
        break;
      case 'containerTypes':
        this.selectedContainerType = this.getFilteredContainerTypes()[0] ?? null;
        break;
      case 'lineOperators':
        this.selectedLineOperator = this.getFilteredLineOperators()[0] ?? null;
        break;
      case 'customers':
        this.selectedCustomer = this.getFilteredCustomers()[0] ?? null;
        break;
    }
  }

  private clearSelections(): void {
    this.selectedBlock = null;
    this.selectedContainerType = null;
    this.selectedLineOperator = null;
    this.selectedCustomer = null;
  }

  private getNormalizedSearchKeyword(): string {
    return this.searchKeyword.trim().toLowerCase();
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

    if (Array.isArray(responseBody?.errors) && responseBody.errors.length > 0) {
      return responseBody.errors[0]?.message ?? fallbackMessage;
    }

    if (Array.isArray(responseBody?.Errors) && responseBody.Errors.length > 0) {
      return responseBody.Errors[0]?.Message ?? fallbackMessage;
    }

    return fallbackMessage;
  }
}
