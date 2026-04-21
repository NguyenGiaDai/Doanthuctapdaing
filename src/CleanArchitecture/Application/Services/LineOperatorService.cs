using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.LineOperator;

namespace CleanArchitecture.Application.Services;

public class LineOperatorService(IUnitOfWork unitOfWork, IMapper mapper) : ILineOperatorService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<LineOperatorResponse>> GetAll(int pageNumber, int pageSize)
    {
        return await _unitOfWork.LineOperatorRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.LineOperatorCode,
            ascending: true,
            selector: x => new LineOperatorResponse
            {
                Id = x.Id,
                LineOperatorCode = x.LineOperatorCode,
                LineOperatorName = x.LineOperatorName
            });
    }

    public async Task<LineOperatorResponse> Get(int id)
    {
        var lineOperator = await _unitOfWork.LineOperatorRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (lineOperator == null)
            throw new Exception("Line operator not found");

        return _mapper.Map<LineOperatorResponse>(lineOperator);
    }

    public async Task<int> Create(CreateLineOperatorRequest request)
    {
        var isExist = await _unitOfWork.LineOperatorRepository.AnyAsync(x => x.LineOperatorCode == request.LineOperatorCode);

        if (isExist)
            throw new Exception("Line operator code already exists");

        var lineOperator = _mapper.Map<LineOperator>(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.LineOperatorRepository.AddAsync(lineOperator), CancellationToken.None);

        return lineOperator.Id;
    }

    public async Task Update(int id, UpdateLineOperatorRequest request)
    {
        var lineOperator = await _unitOfWork.LineOperatorRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (lineOperator == null)
            throw new Exception("Line operator not found");

        var isDuplicate = await _unitOfWork.LineOperatorRepository.AnyAsync(x =>
            x.Id != id && x.LineOperatorCode == request.LineOperatorCode);

        if (isDuplicate)
            throw new Exception("Line operator code already exists");

        lineOperator.LineOperatorCode = request.LineOperatorCode;
        lineOperator.LineOperatorName = request.LineOperatorName;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.LineOperatorRepository.Update(lineOperator);
        }, CancellationToken.None);
    }
}
