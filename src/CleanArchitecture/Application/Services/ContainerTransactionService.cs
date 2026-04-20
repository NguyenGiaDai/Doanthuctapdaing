using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerTransaction;
using CleanArchitecture.Shared.Models.Errors;

namespace CleanArchitecture.Application.Services;

public class ContainerTransactionService(IUnitOfWork unitOfWork, IMapper mapper) : IContainerTransactionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

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
            throw BuildValidationException("Container transaction not found");

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
            throw BuildValidationException("Container transaction not found");

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

        if (blockType == "Real")
        {
            if (!maxBay.HasValue || !maxRow.HasValue || !maxTier.HasValue)
                throw BuildValidationException("Real block must have MaxBay, MaxRow and MaxTier");

            if (!bay.HasValue || !row.HasValue || !tier.HasValue)
                throw BuildValidationException($"{prefix} Bay, Row and Tier are required for real block");

            if (bay <= 0 || bay > maxBay.Value)
                throw BuildValidationException($"{prefix} Bay must be between 1 and {maxBay.Value}");

            if (row <= 0 || row > maxRow.Value)
                throw BuildValidationException($"{prefix} Row must be between 1 and {maxRow.Value}");

            if (tier <= 0 || tier > maxTier.Value)
                throw BuildValidationException($"{prefix} Tier must be between 1 and {maxTier.Value}");
        }
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
