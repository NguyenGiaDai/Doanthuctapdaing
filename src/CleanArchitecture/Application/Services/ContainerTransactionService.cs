using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerTransaction;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Shared.Models.Errors;

namespace CleanArchitecture.Application.Services;

public class ContainerTransactionService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ApplicationDbContext context) : IContainerTransactionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ApplicationDbContext _context = context;

    public async Task<Pagination<ContainerTransactionResponse>> GetAll(int pageNumber, int pageSize)
    {
        var transactions = await _unitOfWork.ContainerTransactionRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.Id,
            ascending: true,
            selector: x => new ContainerTransactionResponse
            {
                Id = x.Id,
                ContainerId = x.ContainerId,
                TransactionType = x.TransactionType,
                FromBlockId = x.FromBlockId,
                FromBay = x.FromBay,
                FromRow = x.FromRow,
                FromTier = x.FromTier,
                ToBlockId = x.ToBlockId,
                ToBay = x.ToBay,
                ToRow = x.ToRow,
                ToTier = x.ToTier,
                VehicleNumber = x.VehicleNumber,
                TransactionTime = x.TransactionTime,
                Note = x.Note
            }
        );

        return transactions;
    }

    public async Task<ContainerTransactionResponse> Get(int id)
    {
        var transaction = await _unitOfWork.ContainerTransactionRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (transaction == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container transaction not found");

        return _mapper.Map<ContainerTransactionResponse>(transaction);
    }

    public async Task<int> Create(CreateContainerTransactionRequest request)
    {
        await ValidateRequest(request.ContainerId, request.TransactionType,
            request.FromBlockId, request.FromBay, request.FromRow, request.FromTier,
            request.ToBlockId, request.ToBay, request.ToRow, request.ToTier);

        var transaction = _mapper.Map<ContainerTransaction>(request);

        transaction.TransactionTime = request.TransactionTime ?? DateTime.UtcNow;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.ContainerTransactionRepository.AddAsync(transaction), CancellationToken.None);

        return transaction.Id;
    }

    public async Task Update(int id, UpdateContainerTransactionRequest request)
    {
        var transaction = await _unitOfWork.ContainerTransactionRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (transaction == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container transaction not found");

        await ValidateRequest(request.ContainerId, request.TransactionType,
            request.FromBlockId, request.FromBay, request.FromRow, request.FromTier,
            request.ToBlockId, request.ToBay, request.ToRow, request.ToTier);

        transaction.ContainerId = request.ContainerId;
        transaction.TransactionType = request.TransactionType;
        transaction.FromBlockId = request.FromBlockId;
        transaction.FromBay = request.FromBay;
        transaction.FromRow = request.FromRow;
        transaction.FromTier = request.FromTier;
        transaction.ToBlockId = request.ToBlockId;
        transaction.ToBay = request.ToBay;
        transaction.ToRow = request.ToRow;
        transaction.ToTier = request.ToTier;
        transaction.VehicleNumber = request.VehicleNumber;
        transaction.TransactionTime = request.TransactionTime ?? transaction.TransactionTime;
        transaction.Note = request.Note;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.ContainerTransactionRepository.Update(transaction);
        }, CancellationToken.None);
    }

    public async Task<int> ImportContainer(ImportContainerRequest request)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(
            x => x.Id == request.ContainerId,
            q => q.Include(c => c.ContainerTypeNavigation));

        if (container == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container not found");

        if (container.CurrentStatus == "InYard")
            throw BuildValidationException("Container is already in yard and cannot be imported again");

        var block = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == request.ToBlockId);

        if (block == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Block not found");

        ValidatePositionInBlock(
            block.BlockType,
            block.MaxBay,
            block.MaxRow,
            block.MaxTier,
            request.ToBay,
            request.ToRow,
            request.ToTier,
            "To");

        ValidateContainerAllowedInBlock(block.BlockType, container.ContainerClassification, container.ContainerCondition);

        if (container.ContainerTypeNavigation?.ContainerSize == 20 && request.ToBay % 2 == 0)
            throw BuildValidationException("20ft container must be placed in an odd bay");

        if (container.ContainerTypeNavigation?.ContainerSize == 40 && request.ToBay % 2 != 0)
            throw BuildValidationException("40ft container must be placed in an even bay");

        await ValidatePositionIsEmpty(
            request.ToBlockId,
            request.ToBay,
            request.ToRow,
            request.ToTier,
            request.ContainerId);

        await ValidateTierStackingForPlacement(
            block.BlockType,
            container.ContainerClassification,
            request.ToBlockId,
            request.ToBay,
            request.ToRow,
            request.ToTier,
            request.ContainerId);

        var existingPosition = await _unitOfWork.ContainerPositionRepository
            .FirstOrDefaultAsync(x => x.ContainerId == request.ContainerId);

        var transaction = new ContainerTransaction
        {
            ContainerId = request.ContainerId,
            TransactionType = "In",
            FromBlockId = null,
            FromBay = null,
            FromRow = null,
            FromTier = null,
            ToBlockId = request.ToBlockId,
            ToBay = request.ToBay,
            ToRow = request.ToRow,
            ToTier = request.ToTier,
            VehicleNumber = request.VehicleNumber,
            TransactionTime = request.TransactionTime ?? DateTime.UtcNow,
            Note = request.Note
        };

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await _unitOfWork.ContainerTransactionRepository.AddAsync(transaction);

            if (existingPosition == null)
            {
                var newPosition = new ContainerPosition
                {
                    ContainerId = request.ContainerId,
                    BlockId = request.ToBlockId,
                    Bay = request.ToBay,
                    Row = request.ToRow,
                    Tier = request.ToTier,
                    PositionTime = request.TransactionTime ?? DateTime.UtcNow
                };

                await _unitOfWork.ContainerPositionRepository.AddAsync(newPosition);
            }
            else
            {
                existingPosition.BlockId = request.ToBlockId;
                existingPosition.Bay = request.ToBay;
                existingPosition.Row = request.ToRow;
                existingPosition.Tier = request.ToTier;
                existingPosition.PositionTime = request.TransactionTime ?? DateTime.UtcNow;

                _unitOfWork.ContainerPositionRepository.Update(existingPosition);
            }

            container.CurrentStatus = "InYard";
            _unitOfWork.ContainerRepository.Update(container);
        }, CancellationToken.None);

        return transaction.Id;
    }

    public async Task<int> ExportContainer(ExportContainerRequest request)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == request.ContainerId);

        if (container == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container not found");

        if (container.CurrentStatus != "InYard")
            throw BuildValidationException("Container is not currently in yard and cannot be exported");

        var deliveryOrder = await _unitOfWork.DeliveryOrderRepository
            .FirstOrDefaultAsync(x => x.Id == request.DeliveryOrderId);

        if (deliveryOrder == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Delivery order not found");

        if (deliveryOrder.ExpiryDate.Date < DateTime.UtcNow.Date)
            throw BuildValidationException("Delivery order has expired and cannot be used for export");

        if (container.LineOperatorId != deliveryOrder.LineOperatorId)
            throw BuildValidationException("Delivery order line operator does not match container line operator");

        if (container.ContainerTypeId != deliveryOrder.ContainerTypeId)
            throw BuildValidationException("Delivery order container type does not match container type");

        var currentPosition = await _unitOfWork.ContainerPositionRepository
            .FirstOrDefaultAsync(x => x.ContainerId == request.ContainerId);

        if (currentPosition == null)
            throw BuildValidationException("Container is not currently in yard");

        await ValidateTierStackingForRemoval(
            currentPosition.BlockId,
            currentPosition.Bay,
            currentPosition.Row,
            currentPosition.Tier,
            currentPosition.ContainerId);

        var transaction = new ContainerTransaction
        {
            ContainerId = request.ContainerId,
            TransactionType = "Out",
            FromBlockId = currentPosition.BlockId,
            FromBay = currentPosition.Bay,
            FromRow = currentPosition.Row,
            FromTier = currentPosition.Tier,
            ToBlockId = null,
            ToBay = null,
            ToRow = null,
            ToTier = null,
            VehicleNumber = request.VehicleNumber,
            TransactionTime = request.TransactionTime ?? DateTime.UtcNow,
            Note = request.Note
        };

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await _unitOfWork.ContainerTransactionRepository.AddAsync(transaction);

            _unitOfWork.ContainerPositionRepository.Delete(currentPosition);

            container.CurrentStatus = "OutYard";
            _unitOfWork.ContainerRepository.Update(container);
        }, CancellationToken.None);

        return transaction.Id;
    }

    public async Task<List<ContainerThroughputReportResponse>> GetContainerThroughputReport(ContainerThroughputReportRequest request)
    {
        var startDate = request.Date.Date;
        var endDate = startDate.AddDays(1);

        var report = await _context.ContainerTransactions
            .Include(x => x.Container)
            .ThenInclude(c => c.LineOperator)
            .Where(x => x.TransactionTime >= startDate && x.TransactionTime < endDate)
            .GroupBy(x => new
            {
                x.Container.LineOperatorId,
                LineOperatorCode = x.Container.LineOperator != null ? x.Container.LineOperator.LineOperatorCode : string.Empty,
                LineOperatorName = x.Container.LineOperator != null ? x.Container.LineOperator.LineOperatorName : string.Empty
            })
            .Select(g => new ContainerThroughputReportResponse
            {
                LineOperatorId = g.Key.LineOperatorId,
                LineOperatorCode = g.Key.LineOperatorCode,
                LineOperatorName = g.Key.LineOperatorName,
                ImportCount = g.Count(x => x.TransactionType == "In"),
                ExportCount = g.Count(x => x.TransactionType == "Out"),
                TotalCount = g.Count()
            })
            .OrderBy(x => x.LineOperatorCode)
            .ToListAsync();

        return report;
    }

    public async Task<List<ContainerYardInventoryReportResponse>> GetContainerYardInventoryReport(ContainerYardInventoryReportRequest request)
    {
        var reportDate = request.Date.Date;

        var containersInYard = await _context.Containers
            .Include(x => x.LineOperator)
            .Include(x => x.ContainerTransactions)
            .Where(x => x.CurrentStatus == "InYard")
            .ToListAsync();

        var report = containersInYard
            .Select(container =>
            {
                var firstImport = container.ContainerTransactions
                    .Where(t => t.TransactionType == "In" && t.TransactionTime <= reportDate.AddDays(1).AddTicks(-1))
                    .OrderBy(t => t.TransactionTime)
                    .FirstOrDefault();

                var daysInYard = firstImport == null
                    ? 0
                    : (reportDate - firstImport.TransactionTime.Date).Days;

                return new
                {
                    container.LineOperatorId,
                    LineOperatorCode = container.LineOperator != null ? container.LineOperator.LineOperatorCode : string.Empty,
                    LineOperatorName = container.LineOperator != null ? container.LineOperator.LineOperatorName : string.Empty,
                    DaysInYard = daysInYard
                };
            })
            .GroupBy(x => new
            {
                x.LineOperatorId,
                x.LineOperatorCode,
                x.LineOperatorName
            })
            .Select(g => new ContainerYardInventoryReportResponse
            {
                LineOperatorId = g.Key.LineOperatorId,
                LineOperatorCode = g.Key.LineOperatorCode,
                LineOperatorName = g.Key.LineOperatorName,
                From0To10DaysCount = g.Count(x => x.DaysInYard >= 0 && x.DaysInYard < 10),
                From10DaysOrMoreCount = g.Count(x => x.DaysInYard >= 10),
                TotalInYardCount = g.Count()
            })
            .OrderBy(x => x.LineOperatorCode)
            .ToList();

        return report;
    }

    private async Task ValidateRequest(
        int containerId,
        string transactionType,
        int? fromBlockId,
        int? fromBay,
        int? fromRow,
        int? fromTier,
        int? toBlockId,
        int? toBay,
        int? toRow,
        int? toTier)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == containerId);

        if (container == null)
            throw BuildValidationException("Container not found");

        if (string.IsNullOrWhiteSpace(transactionType))
            throw BuildValidationException("Transaction type is required");

        if (transactionType != "In" && transactionType != "Out" && transactionType != "Move")
            throw BuildValidationException("Transaction type must be In, Out or Move");

        if (transactionType == "In")
        {
            if (!toBlockId.HasValue)
                throw BuildValidationException("In transaction must have ToBlockId");

            if (fromBlockId.HasValue)
                throw BuildValidationException("In transaction should not have FromBlockId");
        }

        if (transactionType == "Out")
        {
            if (!fromBlockId.HasValue)
                throw BuildValidationException("Out transaction must have FromBlockId");

            if (toBlockId.HasValue)
                throw BuildValidationException("Out transaction should not have ToBlockId");
        }

        if (transactionType == "Move")
        {
            if (!fromBlockId.HasValue || !toBlockId.HasValue)
                throw BuildValidationException("Move transaction must have both FromBlockId and ToBlockId");
        }

        if (fromBlockId.HasValue)
        {
            var fromBlock = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == fromBlockId.Value);

            if (fromBlock == null)
                throw BuildValidationException("From block not found");

            ValidatePositionInBlock(
                fromBlock.BlockType,
                fromBlock.MaxBay,
                fromBlock.MaxRow,
                fromBlock.MaxTier,
                fromBay,
                fromRow,
                fromTier,
                "From");

            if (IsPhysicalBlockType(fromBlock.BlockType))
            {
                await ValidateTierStackingForRemoval(
                    fromBlockId.Value,
                    fromBay!.Value,
                    fromRow!.Value,
                    fromTier!.Value,
                    containerId);
            }
        }

        if (toBlockId.HasValue)
        {
            var toBlock = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == toBlockId.Value);

            if (toBlock == null)
                throw BuildValidationException("To block not found");

            ValidatePositionInBlock(
                toBlock.BlockType,
                toBlock.MaxBay,
                toBlock.MaxRow,
                toBlock.MaxTier,
                toBay,
                toRow,
                toTier,
                "To");

            ValidateContainerAllowedInBlock(toBlock.BlockType, container.ContainerClassification, container.ContainerCondition);

            if (IsPhysicalBlockType(toBlock.BlockType))
            {
                await ValidatePositionIsEmpty(
                    toBlockId.Value,
                    toBay!.Value,
                    toRow!.Value,
                    toTier!.Value,
                    containerId);

                await ValidateTierStackingForPlacement(
                    toBlock.BlockType,
                    container.ContainerClassification,
                    toBlockId.Value,
                    toBay.Value,
                    toRow.Value,
                    toTier.Value,
                    containerId);
            }
        }
    }

    private static void ValidatePositionInBlock(
        string blockType,
        int? maxBay,
        int? maxRow,
        int? maxTier,
        int? bay,
        int? row,
        int? tier,
        string prefix)
    {
        if (string.IsNullOrWhiteSpace(blockType))
            throw BuildValidationException("Block type is required");

        if (!IsValidBlockType(blockType))
            throw BuildValidationException("Block type must be Normal, Special, Electric, Damaged or Virtual");

        if (IsPhysicalBlockType(blockType))
        {
            if (!maxBay.HasValue || !maxRow.HasValue || !maxTier.HasValue)
                throw BuildValidationException("Normal, Special, Electric and Damaged blocks must have MaxBay, MaxRow and MaxTier");

            if (!bay.HasValue || !row.HasValue || !tier.HasValue)
                throw BuildValidationException($"{prefix} Bay, Row and Tier are required for Normal, Special, Electric and Damaged blocks");

            if (bay <= 0 || bay > maxBay.Value)
                throw BuildValidationException($"{prefix} Bay must be between 1 and {maxBay.Value}");

            if (row <= 0 || row > maxRow.Value)
                throw BuildValidationException($"{prefix} Row must be between 1 and {maxRow.Value}");

            if (tier <= 0 || tier > maxTier.Value)
                throw BuildValidationException($"{prefix} Tier must be between 1 and {maxTier.Value}");
        }

        if (IsVirtualBlockType(blockType))
        {
            if (maxBay.HasValue || maxRow.HasValue || maxTier.HasValue)
                throw BuildValidationException("Virtual block must not have MaxBay, MaxRow or MaxTier");
        }
    }

    private async Task ValidatePositionIsEmpty(
        int blockId,
        int bay,
        int row,
        int tier,
        int containerId)
    {
        var occupiedPosition = await _unitOfWork.ContainerPositionRepository.FirstOrDefaultAsync(
            x => x.BlockId == blockId
                 && x.Bay == bay
                 && x.Row == row
                 && x.Tier == tier
                 && x.ContainerId != containerId);

        if (occupiedPosition != null)
            throw BuildValidationException("This yard position is already occupied by another container");
    }

    private async Task ValidateTierStackingForPlacement(
        string blockType,
        string? containerClassification,
        int blockId,
        int bay,
        int row,
        int targetTier,
        int containerId)
    {
        if (!IsPhysicalBlockType(blockType))
            return;

        if (string.Equals(containerClassification, "B", StringComparison.OrdinalIgnoreCase)
            && targetTier > 1)
            throw BuildValidationException("Classification B containers can only be placed at tier 1");

        if (targetTier <= 1)
            return;

        for (var tier = 1; tier < targetTier; tier++)
        {
            var lowerPosition = await _unitOfWork.ContainerPositionRepository.FirstOrDefaultAsync(
                x => x.BlockId == blockId
                     && x.Bay == bay
                     && x.Row == row
                     && x.Tier == tier
                     && x.ContainerId != containerId);

            if (lowerPosition == null)
                throw BuildValidationException(
                    $"Cannot place container at tier {targetTier} because tier {tier} below is empty at the same block, bay and row");
        }
    }

    private async Task ValidateTierStackingForRemoval(
        int blockId,
        int bay,
        int row,
        int currentTier,
        int containerId)
    {
        var upperPosition = await _unitOfWork.ContainerPositionRepository.FirstOrDefaultAsync(
            x => x.BlockId == blockId
                 && x.Bay == bay
                 && x.Row == row
                 && x.Tier > currentTier
                 && x.ContainerId != containerId);

        if (upperPosition != null)
            throw BuildValidationException(
                $"Cannot remove container from tier {currentTier} because another container is stacked above it at tier {upperPosition.Tier}");
    }

    private static void ValidateContainerAllowedInBlock(
        string blockType,
        string containerClassification,
        string containerCondition)
    {
        if (IsVirtualBlockType(blockType))
            return;

        if (string.Equals(blockType, "Normal", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(containerClassification, "A", StringComparison.OrdinalIgnoreCase))
            throw BuildValidationException("Normal block can only contain classification A containers");

        if (string.Equals(blockType, "Special", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(containerClassification, "B", StringComparison.OrdinalIgnoreCase))
            throw BuildValidationException("Special block can only contain classification B containers");

        if (string.Equals(blockType, "Electric", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(containerClassification, "C", StringComparison.OrdinalIgnoreCase))
            throw BuildValidationException("Electric block can only contain classification C containers");

        if (string.Equals(blockType, "Damaged", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(containerCondition, "Damaged", StringComparison.OrdinalIgnoreCase))
            throw BuildValidationException("Damaged block can only contain damaged containers");
    }

    private static bool IsValidBlockType(string blockType)
    {
        return IsPhysicalBlockType(blockType) || IsVirtualBlockType(blockType);
    }

    private static bool IsPhysicalBlockType(string blockType)
    {
        return string.Equals(blockType, "Normal", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blockType, "Special", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blockType, "Electric", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blockType, "Damaged", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVirtualBlockType(string blockType)
    {
        return string.Equals(blockType, "Virtual", StringComparison.OrdinalIgnoreCase);
    }

    private static ValidationException BuildValidationException(string message)
    {
        return new ValidationException(
            new ErrorResponse(
                [
                    new Error("Validation.ContainerTransaction", message)
                ]
            )
        );
    }
}
