using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Block;
using CleanArchitecture.Shared.Models.Errors;

namespace CleanArchitecture.Application.Services;

public class BlockService(IUnitOfWork unitOfWork, IMapper mapper) : IBlockService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<BlockResponse>> GetAll(int pageNumber, int pageSize)
    {
        var blocks = await _unitOfWork.BlockRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.BlockCode,
            ascending: true,
            selector: x => new BlockResponse
            {
                Id = x.Id,
                DepotId = x.DepotId,
                DepotCode = string.Empty,
                DepotName = string.Empty,
                BlockCode = x.BlockCode,
                BlockName = x.BlockName,
                BlockType = x.BlockType,
                MaxBay = x.MaxBay,
                MaxRow = x.MaxRow,
                MaxTier = x.MaxTier
            }
        );

        return blocks;
    }

    public async Task<BlockResponse> Get(int id)
    {
        var block = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (block == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Block not found");

        return _mapper.Map<BlockResponse>(block);
    }

    public async Task<int> Create(CreateBlockRequest request)
    {
        var depot = await _unitOfWork.DepotRepository.FirstOrDefaultAsync(x => x.Id == request.DepotId);

        if (depot == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Depot not found");

        var normalizedBlockType = NormalizeBlockType(request.BlockType);
        ValidateBlockType(normalizedBlockType, request.MaxBay, request.MaxRow, request.MaxTier);

        var isExist = await _unitOfWork.BlockRepository.AnyAsync(x =>
            x.DepotId == request.DepotId && x.BlockCode == request.BlockCode);

        if (isExist)
            throw new UserFriendlyException(ErrorCode.Conflict, "Block code already exists in this depot");

        var block = _mapper.Map<Block>(request);
        block.BlockType = normalizedBlockType;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.BlockRepository.AddAsync(block), CancellationToken.None);

        return block.Id;
    }

    public async Task Update(int id, UpdateBlockRequest request)
    {
        var block = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (block == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Block not found");

        var normalizedBlockType = NormalizeBlockType(request.BlockType);
        ValidateBlockType(normalizedBlockType, request.MaxBay, request.MaxRow, request.MaxTier);

        var depot = await _unitOfWork.DepotRepository.FirstOrDefaultAsync(x => x.Id == request.DepotId);

        if (depot == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Depot not found");

        var isDuplicate = await _unitOfWork.BlockRepository.AnyAsync(x =>
            x.Id != id &&
            x.DepotId == request.DepotId &&
            x.BlockCode == request.BlockCode);

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "Block code already exists in this depot");

        block.DepotId = request.DepotId;
        block.BlockCode = request.BlockCode;
        block.BlockName = request.BlockName;
        block.BlockType = normalizedBlockType;
        block.MaxBay = request.MaxBay;
        block.MaxRow = request.MaxRow;
        block.MaxTier = request.MaxTier;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.BlockRepository.Update(block);
        }, CancellationToken.None);
    }

    private static void ValidateBlockType(string blockType, int? maxBay, int? maxRow, int? maxTier)
    {
        if (string.IsNullOrWhiteSpace(blockType))
            throw BuildValidationException("Block type is required");

        if (!IsValidBlockType(blockType))
            throw BuildValidationException("Block type must be Normal, Special, Electric, Damaged or Virtual");

        if (IsPhysicalBlockType(blockType))
        {
            if (!maxBay.HasValue || !maxRow.HasValue || !maxTier.HasValue)
                throw BuildValidationException("Normal, Special, Electric and Damaged blocks must have MaxBay, MaxRow and MaxTier");

            if (maxBay <= 0 || maxRow <= 0 || maxTier <= 0)
                throw BuildValidationException("MaxBay, MaxRow and MaxTier must be greater than 0 for Normal, Special, Electric and Damaged blocks");
        }

        if (IsVirtualBlockType(blockType))
        {
            if (maxBay.HasValue || maxRow.HasValue || maxTier.HasValue)
                throw BuildValidationException("Virtual block must not have MaxBay, MaxRow or MaxTier");
        }
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

    private static string NormalizeBlockType(string blockType)
    {
        if (string.IsNullOrWhiteSpace(blockType))
            return string.Empty;

        if (string.Equals(blockType, "Normal", StringComparison.OrdinalIgnoreCase))
            return "Normal";

        if (string.Equals(blockType, "Special", StringComparison.OrdinalIgnoreCase))
            return "Special";

        if (string.Equals(blockType, "Electric", StringComparison.OrdinalIgnoreCase))
            return "Electric";

        if (string.Equals(blockType, "Damaged", StringComparison.OrdinalIgnoreCase))
            return "Damaged";

        if (string.Equals(blockType, "Virtual", StringComparison.OrdinalIgnoreCase))
            return "Virtual";

        return blockType.Trim();
    }

    private static ValidationException BuildValidationException(string message)
    {
        return new ValidationException(
            new ErrorResponse(
                [
                    new Error("Validation.Block", message)
                ]
            )
        );
    }
}
