using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
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
            throw new UserFriendlyException(ErrorCode.NotFound, "Line operator not found");

        return _mapper.Map<LineOperatorResponse>(lineOperator);
    }

    public async Task<int> Create(CreateLineOperatorRequest request)
    {
        ValidateRequest(request.LineOperatorCode, request.LineOperatorName);

        var normalizedCode = request.LineOperatorCode.Trim().ToUpper();
        var normalizedName = request.LineOperatorName.Trim();

        var isExist = await _unitOfWork.LineOperatorRepository.AnyAsync(x => x.LineOperatorCode == normalizedCode);

        if (isExist)
            throw new UserFriendlyException(ErrorCode.Conflict, "Line operator code already exists");

        var lineOperator = _mapper.Map<LineOperator>(request);
        lineOperator.LineOperatorCode = normalizedCode;
        lineOperator.LineOperatorName = normalizedName;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.LineOperatorRepository.AddAsync(lineOperator), CancellationToken.None);

        return lineOperator.Id;
    }

    public async Task Update(int id, UpdateLineOperatorRequest request)
    {
        ValidateRequest(request.LineOperatorCode, request.LineOperatorName);

        var lineOperator = await _unitOfWork.LineOperatorRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (lineOperator == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Line operator not found");

        var normalizedCode = request.LineOperatorCode.Trim().ToUpper();
        var normalizedName = request.LineOperatorName.Trim();

        var isDuplicate = await _unitOfWork.LineOperatorRepository.AnyAsync(x =>
            x.Id != id && x.LineOperatorCode == normalizedCode);

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "Line operator code already exists");

        lineOperator.LineOperatorCode = normalizedCode;
        lineOperator.LineOperatorName = normalizedName;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.LineOperatorRepository.Update(lineOperator);
        }, CancellationToken.None);
    }

    private static void ValidateRequest(string lineOperatorCode, string lineOperatorName)
    {
        if (string.IsNullOrWhiteSpace(lineOperatorCode))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Line operator code is required");

        if (string.IsNullOrWhiteSpace(lineOperatorName))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Line operator name is required");
    }
}
