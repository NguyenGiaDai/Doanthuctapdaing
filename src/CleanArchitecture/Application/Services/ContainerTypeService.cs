using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerType;

namespace CleanArchitecture.Application.Services;

public class ContainerTypeService(IUnitOfWork unitOfWork, IMapper mapper) : IContainerTypeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<ContainerTypeResponse>> GetAll(int pageNumber, int pageSize)
    {
        return await _unitOfWork.ContainerTypeRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.ContainerTypeCode,
            ascending: true,
            selector: x => new ContainerTypeResponse
            {
                Id = x.Id,
                ContainerTypeCode = x.ContainerTypeCode,
                ContainerTypeName = x.ContainerTypeName,
                ISOCode = x.ISOCode,
                ContainerSize = x.ContainerSize,
                MaximumWeight = x.MaximumWeight,
                TareWeight = x.TareWeight
            });
    }

    public async Task<ContainerTypeResponse> Get(int id)
    {
        var containerType = await _unitOfWork.ContainerTypeRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (containerType == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container type not found");

        return _mapper.Map<ContainerTypeResponse>(containerType);
    }

    public async Task<int> Create(CreateContainerTypeRequest request)
    {
        ValidateRequest(
            request.ContainerTypeCode,
            request.ContainerTypeName,
            request.ISOCode,
            request.ContainerSize,
            request.MaximumWeight,
            request.TareWeight);

        var normalizedCode = request.ContainerTypeCode.Trim().ToUpper();
        var normalizedName = request.ContainerTypeName.Trim();
        var normalizedIsoCode = request.ISOCode.Trim().ToUpper();

        var isExist = await _unitOfWork.ContainerTypeRepository.AnyAsync(x => x.ContainerTypeCode == normalizedCode);

        if (isExist)
            throw new UserFriendlyException(ErrorCode.Conflict, "Container type code already exists");

        var containerType = _mapper.Map<ContainerType>(request);
        containerType.ContainerTypeCode = normalizedCode;
        containerType.ContainerTypeName = normalizedName;
        containerType.ISOCode = normalizedIsoCode;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.ContainerTypeRepository.AddAsync(containerType), CancellationToken.None);

        return containerType.Id;
    }

    public async Task Update(int id, UpdateContainerTypeRequest request)
    {
        ValidateRequest(
            request.ContainerTypeCode,
            request.ContainerTypeName,
            request.ISOCode,
            request.ContainerSize,
            request.MaximumWeight,
            request.TareWeight);

        var containerType = await _unitOfWork.ContainerTypeRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (containerType == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container type not found");

        var normalizedCode = request.ContainerTypeCode.Trim().ToUpper();
        var normalizedName = request.ContainerTypeName.Trim();
        var normalizedIsoCode = request.ISOCode.Trim().ToUpper();

        var isDuplicate = await _unitOfWork.ContainerTypeRepository.AnyAsync(x =>
            x.Id != id && x.ContainerTypeCode == normalizedCode);

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "Container type code already exists");

        containerType.ContainerTypeCode = normalizedCode;
        containerType.ContainerTypeName = normalizedName;
        containerType.ISOCode = normalizedIsoCode;
        containerType.ContainerSize = request.ContainerSize;
        containerType.MaximumWeight = request.MaximumWeight;
        containerType.TareWeight = request.TareWeight;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.ContainerTypeRepository.Update(containerType);
        }, CancellationToken.None);
    }

    private static void ValidateRequest(
        string containerTypeCode,
        string containerTypeName,
        string isoCode,
        int containerSize,
        decimal? maximumWeight,
        decimal? tareWeight)
    {
        if (string.IsNullOrWhiteSpace(containerTypeCode))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Container type code is required");

        if (string.IsNullOrWhiteSpace(containerTypeName))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Container type name is required");

        if (string.IsNullOrWhiteSpace(isoCode))
            throw new UserFriendlyException(ErrorCode.BadRequest, "ISO code is required");

        if (containerSize <= 0)
            throw new UserFriendlyException(ErrorCode.BadRequest, "Container size must be greater than 0");

        if (maximumWeight.HasValue && maximumWeight.Value <= 0)
            throw new UserFriendlyException(ErrorCode.BadRequest, "Maximum weight must be greater than 0");

        if (tareWeight.HasValue && tareWeight.Value <= 0)
            throw new UserFriendlyException(ErrorCode.BadRequest, "Tare weight must be greater than 0");
    }
}
