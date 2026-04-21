using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Container;
using Microsoft.EntityFrameworkCore;

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
            include: query => query
                .Include(x => x.ContainerTypeNavigation)
                .Include(x => x.LineOperator),
            orderBy: x => x.ContainerNumber,
            ascending: true,
            selector: x => new ContainerResponse
            {
                Id = x.Id,
                ContainerNumber = x.ContainerNumber,

                ContainerTypeId = x.ContainerTypeId,
                ContainerTypeCode = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ContainerTypeCode : string.Empty,
                ContainerTypeName = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ContainerTypeName : string.Empty,
                ISOCode = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ISOCode : string.Empty,
                ContainerSize = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ContainerSize : 0,
                MaximumWeight = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.MaximumWeight : null,
                TareWeight = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.TareWeight : null,

                LineOperatorId = x.LineOperatorId,
                LineOperatorCode = x.LineOperator != null ? x.LineOperator.LineOperatorCode : string.Empty,
                LineOperatorName = x.LineOperator != null ? x.LineOperator.LineOperatorName : string.Empty,

                DateOfManufacture = x.DateOfManufacture,
                ContainerOwner = x.ContainerOwner,
                ContainerCondition = x.ContainerCondition,
                ContainerClassification = x.ContainerClassification,
                CurrentStatus = x.CurrentStatus
            }
        );

        return containers;
    }

    public async Task<ContainerResponse> Get(int id)
    {
        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(
            x => x.Id == id,
            include: query => query
                .Include(x => x.ContainerTypeNavigation)
                .Include(x => x.LineOperator));

        if (container == null)
            throw new Exception("Container not found");

        return _mapper.Map<ContainerResponse>(container);
    }

    public async Task<ContainerResponse> Add(CreateContainerRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.ContainerNumber))
            throw new Exception("Container number is required");

        var isExist = await _unitOfWork.ContainerRepository.AnyAsync(
            x => x.ContainerNumber == request.ContainerNumber.Trim());

        if (isExist)
            throw new Exception("Container number already exists");

        var containerTypeExists = await _unitOfWork.ContainerTypeRepository.AnyAsync(
            x => x.Id == request.ContainerTypeId);

        if (!containerTypeExists)
            throw new Exception("Container type not found");

        var lineOperatorExists = await _unitOfWork.LineOperatorRepository.AnyAsync(
            x => x.Id == request.LineOperatorId);

        if (!lineOperatorExists)
            throw new Exception("Line operator not found");

        var container = _mapper.Map<Container>(request);
        container.ContainerNumber = request.ContainerNumber.Trim();

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.ContainerRepository.AddAsync(container), token);

        var createdContainer = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(
            x => x.Id == container.Id,
            include: query => query
                .Include(x => x.ContainerTypeNavigation)
                .Include(x => x.LineOperator));

        return _mapper.Map<ContainerResponse>(createdContainer);
    }

    public async Task<ContainerResponse> Update(UpdateContainerRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.ContainerNumber))
            throw new Exception("Container number is required");

        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (container == null)
            throw new Exception("Container not found");

        var isDuplicate = await _unitOfWork.ContainerRepository.AnyAsync(x =>
            x.Id != request.Id && x.ContainerNumber == request.ContainerNumber.Trim());

        if (isDuplicate)
            throw new Exception("Container number already exists");

        var containerTypeExists = await _unitOfWork.ContainerTypeRepository.AnyAsync(
            x => x.Id == request.ContainerTypeId);

        if (!containerTypeExists)
            throw new Exception("Container type not found");

        var lineOperatorExists = await _unitOfWork.LineOperatorRepository.AnyAsync(
            x => x.Id == request.LineOperatorId);

        if (!lineOperatorExists)
            throw new Exception("Line operator not found");

        container.ContainerNumber = request.ContainerNumber.Trim();
        container.ContainerTypeId = request.ContainerTypeId;
        container.LineOperatorId = request.LineOperatorId;
        container.DateOfManufacture = request.DateOfManufacture;
        container.ContainerOwner = request.ContainerOwner;
        container.ContainerCondition = request.ContainerCondition;
        container.ContainerClassification = request.ContainerClassification;
        container.CurrentStatus = request.CurrentStatus;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.ContainerRepository.Update(container);
        }, token);

        var updatedContainer = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            include: query => query
                .Include(x => x.ContainerTypeNavigation)
                .Include(x => x.LineOperator));

        return _mapper.Map<ContainerResponse>(updatedContainer);
    }
}
