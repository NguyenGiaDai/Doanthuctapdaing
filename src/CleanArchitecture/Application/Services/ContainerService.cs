using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Container;

namespace CleanArchitecture.Application.Services;

public class ContainerService(IUnitOfWork unitOfWork, IMapper mapper) : IContainerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<ContainerResponse>> Get(int pageIndex, int pageSize)
    {
        var containers = await _unitOfWork.ContainerRepository.ToPagination(
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: x => x.ContainerNumber,
            ascending: true,
            selector: x => new ContainerResponse
            {
                Id = x.Id,
                ContainerNumber = x.ContainerNumber,
                ContainerType = x.ContainerType,
                IsoCode = x.IsoCode,
                ContainerSize = x.ContainerSize,
                MaximumWeight = x.MaximumWeight,
                TareWeight = x.TareWeight,
                DateOfManufacture = x.DateOfManufacture,
                ContainerOwner = x.ContainerOwner,
                ContainerCondition = x.ContainerCondition
            }
        );

        return containers;
    }

    public async Task<ContainerResponse> Get(int id)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (container == null)
            throw new Exception("Container not found");

        return _mapper.Map<ContainerResponse>(container);
    }

    public async Task<ContainerResponse> Add(CreateContainerRequest request, CancellationToken token)
    {
        var isExist = await _unitOfWork.ContainerRepository.AnyAsync(x => x.ContainerNumber == request.ContainerNumber);

        if (isExist)
            throw new Exception("Container number already exists");

        var container = _mapper.Map<Container>(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.ContainerRepository.AddAsync(container), token);

        return _mapper.Map<ContainerResponse>(container);
    }

    public async Task<ContainerResponse> Update(UpdateContainerRequest request, CancellationToken token)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (container == null)
            throw new Exception("Container not found");

        var isDuplicate = await _unitOfWork.ContainerRepository.AnyAsync(x =>
            x.Id != request.Id && x.ContainerNumber == request.ContainerNumber);

        if (isDuplicate)
            throw new Exception("Container number already exists");

        container.ContainerNumber = request.ContainerNumber;
        container.ContainerType = request.ContainerType;
        container.IsoCode = request.IsoCode;
        container.ContainerSize = request.ContainerSize;
        container.MaximumWeight = request.MaximumWeight;
        container.TareWeight = request.TareWeight;
        container.DateOfManufacture = request.DateOfManufacture;
        container.ContainerOwner = request.ContainerOwner;
        container.ContainerCondition = request.ContainerCondition;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.ContainerRepository.Update(container);
        }, token);

        return _mapper.Map<ContainerResponse>(container);
    }
}
