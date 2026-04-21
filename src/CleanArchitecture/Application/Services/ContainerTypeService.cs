using AutoMapper;
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
            throw new Exception("Container type not found");

        return _mapper.Map<ContainerTypeResponse>(containerType);
    }

    public async Task<int> Create(CreateContainerTypeRequest request)
    {
        var isExist = await _unitOfWork.ContainerTypeRepository.AnyAsync(x => x.ContainerTypeCode == request.ContainerTypeCode);

        if (isExist)
            throw new Exception("Container type code already exists");

        var containerType = _mapper.Map<ContainerType>(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.ContainerTypeRepository.AddAsync(containerType), CancellationToken.None);

        return containerType.Id;
    }

    public async Task Update(int id, UpdateContainerTypeRequest request)
    {
        var containerType = await _unitOfWork.ContainerTypeRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (containerType == null)
            throw new Exception("Container type not found");

        var isDuplicate = await _unitOfWork.ContainerTypeRepository.AnyAsync(x =>
            x.Id != id && x.ContainerTypeCode == request.ContainerTypeCode);

        if (isDuplicate)
            throw new Exception("Container type code already exists");

        containerType.ContainerTypeCode = request.ContainerTypeCode;
        containerType.ContainerTypeName = request.ContainerTypeName;
        containerType.ISOCode = request.ISOCode;
        containerType.ContainerSize = request.ContainerSize;
        containerType.MaximumWeight = request.MaximumWeight;
        containerType.TareWeight = request.TareWeight;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.ContainerTypeRepository.Update(containerType);
        }, CancellationToken.None);
    }
}
