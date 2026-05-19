using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Depot;

namespace CleanArchitecture.Application.Services;

public class DepotService(IUnitOfWork unitOfWork, IMapper mapper) : IDepotService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<DepotResponse>> GetAll(int pageNumber, int pageSize)
    {
        var depots = await _unitOfWork.DepotRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.DepotCode,
            ascending: true,
            selector: x => new DepotResponse
            {
                Id = x.Id,
                DepotCode = x.DepotCode,
                DepotName = x.DepotName,
                Address = x.Address
            }
        );

        return depots;
    }

    public async Task<DepotResponse> Get(int id)
    {
        var depot = await _unitOfWork.DepotRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (depot == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Depot not found");

        return _mapper.Map<DepotResponse>(depot);
    }

    public async Task<int> Create(CreateDepotRequest request)
    {
        ValidateRequest(request.DepotCode, request.DepotName, request.Address);

        var normalizedCode = request.DepotCode.Trim().ToUpper();
        var normalizedName = request.DepotName.Trim();
        var normalizedAddress = request.Address.Trim();

        var isExist = await _unitOfWork.DepotRepository.AnyAsync(x => x.DepotCode == normalizedCode);

        if (isExist)
            throw new UserFriendlyException(ErrorCode.Conflict, "Depot code already exists");

        var depot = _mapper.Map<Depot>(request);
        depot.DepotCode = normalizedCode;
        depot.DepotName = normalizedName;
        depot.Address = normalizedAddress;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.DepotRepository.AddAsync(depot), CancellationToken.None);

        return depot.Id;
    }

    public async Task Update(int id, UpdateDepotRequest request)
    {
        ValidateRequest(request.DepotCode, request.DepotName, request.Address);

        var depot = await _unitOfWork.DepotRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (depot == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Depot not found");

        var normalizedCode = request.DepotCode.Trim().ToUpper();
        var normalizedName = request.DepotName.Trim();
        var normalizedAddress = request.Address.Trim();

        var isDuplicate = await _unitOfWork.DepotRepository.AnyAsync(x =>
            x.Id != id && x.DepotCode == normalizedCode);

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "Depot code already exists");

        depot.DepotCode = normalizedCode;
        depot.DepotName = normalizedName;
        depot.Address = normalizedAddress;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.DepotRepository.Update(depot);
        }, CancellationToken.None);
    }

    private static void ValidateRequest(string depotCode, string depotName, string address)
    {
        if (string.IsNullOrWhiteSpace(depotCode))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Depot code is required");

        if (string.IsNullOrWhiteSpace(depotName))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Depot name is required");

        if (string.IsNullOrWhiteSpace(address))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Address is required");
    }
}
