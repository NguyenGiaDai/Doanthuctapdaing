using AutoMapper;
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
            throw new Exception("Depot not found");

        return _mapper.Map<DepotResponse>(depot);
    }

    public async Task<int> Create(CreateDepotRequest request)
    {
        var isExist = await _unitOfWork.DepotRepository.AnyAsync(x => x.DepotCode == request.DepotCode);

        if (isExist)
            throw new Exception("Depot code already exists");

        var depot = _mapper.Map<Depot>(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.DepotRepository.AddAsync(depot), CancellationToken.None);

        return depot.Id;
    }

    public async Task Update(int id, UpdateDepotRequest request)
    {
        var depot = await _unitOfWork.DepotRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (depot == null)
            throw new Exception("Depot not found");

        var isDuplicate = await _unitOfWork.DepotRepository.AnyAsync(x =>
            x.Id != id && x.DepotCode == request.DepotCode);

        if (isDuplicate)
            throw new Exception("Depot code already exists");

        depot.DepotCode = request.DepotCode;
        depot.DepotName = request.DepotName;
        depot.Address = request.Address;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.DepotRepository.Update(depot);
        }, CancellationToken.None);
    }
}
