using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerPosition;
using CleanArchitecture.Shared.Models.Errors;

namespace CleanArchitecture.Application.Services;

public class ContainerPositionService(IUnitOfWork unitOfWork, IMapper mapper) : IContainerPositionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<ContainerPositionResponse>> GetAll(int pageNumber, int pageSize)
    {
        var positions = await _unitOfWork.ContainerPositionRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.Id,
            ascending: true,
            selector: x => new ContainerPositionResponse
            {
                Id = x.Id,
                ContainerId = x.ContainerId,
                BlockId = x.BlockId,
                Bay = x.Bay,
                Row = x.Row,
                Tier = x.Tier,
                PositionTime = x.PositionTime
            }
        );

        return positions;
    }

    public async Task<ContainerPositionResponse> Get(int id)
    {
        var position = await _unitOfWork.ContainerPositionRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (position == null)
            throw BuildValidationException("Container position not found");

        return _mapper.Map<ContainerPositionResponse>(position);
    }

    public async Task<int> Create(CreateContainerPositionRequest request)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == request.ContainerId);

        if (container == null)
            throw BuildValidationException("Container not found");

        var block = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == request.BlockId);

        if (block == null)
            throw BuildValidationException("Block not found");

        ValidatePositionInBlock(block.BlockType, block.MaxBay, block.MaxRow, block.MaxTier, request.Bay, request.Row, request.Tier);

        var position = _mapper.Map<ContainerPosition>(request);

        if (!position.PositionTime.HasValue)
            position.PositionTime = DateTime.UtcNow;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.ContainerPositionRepository.AddAsync(position), CancellationToken.None);

        return position.Id;
    }

    public async Task Update(int id, UpdateContainerPositionRequest request)
    {
        var position = await _unitOfWork.ContainerPositionRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (position == null)
            throw BuildValidationException("Container position not found");

        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == request.ContainerId);

        if (container == null)
            throw BuildValidationException("Container not found");

        var block = await _unitOfWork.BlockRepository.FirstOrDefaultAsync(x => x.Id == request.BlockId);

        if (block == null)
            throw BuildValidationException("Block not found");

        ValidatePositionInBlock(block.BlockType, block.MaxBay, block.MaxRow, block.MaxTier, request.Bay, request.Row, request.Tier);

        position.ContainerId = request.ContainerId;
        position.BlockId = request.BlockId;
        position.Bay = request.Bay;
        position.Row = request.Row;
        position.Tier = request.Tier;
        position.PositionTime = request.PositionTime ?? position.PositionTime ?? DateTime.UtcNow;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.ContainerPositionRepository.Update(position);
        }, CancellationToken.None);
    }

    private static void ValidatePositionInBlock(
        string blockType,
        int? maxBay,
        int? maxRow,
        int? maxTier,
        int bay,
        int row,
        int tier)
    {
        if (string.IsNullOrWhiteSpace(blockType))
            throw BuildValidationException("Block type is required");

        if (blockType == "Real")
        {
            if (!maxBay.HasValue || !maxRow.HasValue || !maxTier.HasValue)
                throw BuildValidationException("Real block must have MaxBay, MaxRow and MaxTier");

            if (bay <= 0 || bay > maxBay.Value)
                throw BuildValidationException($"Bay must be between 1 and {maxBay.Value}");

            if (row <= 0 || row > maxRow.Value)
                throw BuildValidationException($"Row must be between 1 and {maxRow.Value}");

            if (tier <= 0 || tier > maxTier.Value)
                throw BuildValidationException($"Tier must be between 1 and {maxTier.Value}");
        }
    }

    private static ValidationException BuildValidationException(string message)
    {
        return new ValidationException(
            new ErrorResponse(
                [
                    new Error("Validation.ContainerPosition", message)
                ]
            )
        );
    }
}
