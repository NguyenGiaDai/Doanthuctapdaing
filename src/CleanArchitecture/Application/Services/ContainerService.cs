using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Container;
using System.Text.RegularExpressions;
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

                ContainerTypeId = x.ContainerTypeId ?? 0,
                ContainerTypeCode = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ContainerTypeCode : string.Empty,
                ContainerTypeName = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ContainerTypeName : string.Empty,
                ISOCode = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ISOCode : string.Empty,
                ContainerSize = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.ContainerSize : 0,
                MaximumWeight = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.MaximumWeight : null,
                TareWeight = x.ContainerTypeNavigation != null ? x.ContainerTypeNavigation.TareWeight : null,

                LineOperatorId = x.LineOperatorId ?? 0,
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
            throw new UserFriendlyException(ErrorCode.NotFound, "Container not found");

        return _mapper.Map<ContainerResponse>(container);
    }

    public async Task<ContainerResponse> Add(CreateContainerRequest request, CancellationToken token)
    {
        request.ContainerNumber = request.ContainerNumber?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(request.ContainerNumber))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Container number is required");

        if (!IsValidContainerNumberFormat(request.ContainerNumber))
            throw new UserFriendlyException(
                ErrorCode.BadRequest,
                "Container number is invalid. Expected structure: 3-letter owner code + 1-letter type code + 6-digit serial + 1 check digit (0-9 or X), e.g. CMAU1234567");

        var isExist = await _unitOfWork.ContainerRepository.AnyAsync(
            x => x.ContainerNumber == request.ContainerNumber.Trim());

        if (isExist)
            throw new UserFriendlyException(ErrorCode.Conflict, "Container number already exists");

        var containerTypeExists = await _unitOfWork.ContainerTypeRepository.AnyAsync(
            x => x.Id == request.ContainerTypeId);

        if (!containerTypeExists)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container type not found");

        var lineOperatorExists = await _unitOfWork.LineOperatorRepository.AnyAsync(
            x => x.Id == request.LineOperatorId);

        if (!lineOperatorExists)
            throw new UserFriendlyException(ErrorCode.NotFound, "Line operator not found");

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
        request.ContainerNumber = request.ContainerNumber?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(request.ContainerNumber))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Container number is required");

        if (!IsValidContainerNumberFormat(request.ContainerNumber))
            throw new UserFriendlyException(
                ErrorCode.BadRequest,
                "Container number is invalid. Expected structure: 3-letter owner code + 1-letter type code + 6-digit serial + 1 check digit (0-9 or X), e.g. CMAU1234567");

        var container = await _unitOfWork.ContainerRepository.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (container == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container not found");

        var isDuplicate = await _unitOfWork.ContainerRepository.AnyAsync(x =>
            x.Id != request.Id && x.ContainerNumber == request.ContainerNumber.Trim());

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "Container number already exists");

        var containerTypeExists = await _unitOfWork.ContainerTypeRepository.AnyAsync(
            x => x.Id == request.ContainerTypeId);

        if (!containerTypeExists)
            throw new UserFriendlyException(ErrorCode.NotFound, "Container type not found");

        var lineOperatorExists = await _unitOfWork.LineOperatorRepository.AnyAsync(
            x => x.Id == request.LineOperatorId);

        if (!lineOperatorExists)
            throw new UserFriendlyException(ErrorCode.NotFound, "Line operator not found");

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

    private static bool IsValidContainerNumberFormat(string containerNumber)
    {
        if (string.IsNullOrWhiteSpace(containerNumber))
            return false;

        var normalized = containerNumber.Trim().ToUpper();

        if (!Regex.IsMatch(normalized, @"^[A-Z]{4}\d{6}[0-9X]$"))
            return false;

        var ownerCode = normalized.Substring(0, 3);
        var typeCode = normalized[3];
        var serialNumber = normalized.Substring(4, 6);
        var checkDigitChar = normalized[10];

        if (!IsValidOwnerCode(ownerCode))
            return false;

        if (!IsValidTypeCode(typeCode))
            return false;

        if (!Regex.IsMatch(serialNumber, @"^\d{6}$"))
            return false;

        return IsValidCheckDigit(normalized, checkDigitChar);
    }

    private static bool IsValidOwnerCode(string ownerCode)
    {
        return Regex.IsMatch(ownerCode, @"^[A-Z]{3}$");
    }

    private static bool IsValidTypeCode(char typeCode)
    {
        var allowedTypeCodes = new[] { 'U', 'R', 'S', 'F', 'B', 'V', 'P' };
        return allowedTypeCodes.Contains(char.ToUpper(typeCode));
    }

    private static bool IsValidCheckDigit(string normalizedContainerNumber, char actualCheckDigit)
    {
        var baseCode = normalizedContainerNumber.Substring(0, 10);
        var expectedCheckDigit = CalculateCheckDigit(baseCode);

        return char.ToUpper(actualCheckDigit) == expectedCheckDigit;
    }

    private static char CalculateCheckDigit(string baseCode)
    {
        long sum = 0;

        for (int i = 0; i < baseCode.Length; i++)
        {
            int value = GetIso6346CharacterValue(baseCode[i]);
            sum += value * (long)Math.Pow(2, i);
        }

        var remainder = sum % 11;

        if (remainder == 10)
            return 'X';

        return remainder.ToString()[0];
    }

    private static int GetIso6346CharacterValue(char c)
    {
        if (char.IsDigit(c))
            return c - '0';

        c = char.ToUpper(c);

        return c switch
        {
            'A' => 10,
            'B' => 12,
            'C' => 13,
            'D' => 14,
            'E' => 15,
            'F' => 16,
            'G' => 17,
            'H' => 18,
            'I' => 19,
            'J' => 20,
            'K' => 21,
            'L' => 23,
            'M' => 24,
            'N' => 25,
            'O' => 26,
            'P' => 27,
            'Q' => 28,
            'R' => 29,
            'S' => 30,
            'T' => 31,
            'U' => 32,
            'V' => 34,
            'W' => 35,
            'X' => 36,
            'Y' => 37,
            'Z' => 38,
            _ => throw new UserFriendlyException(ErrorCode.BadRequest, $"Invalid character in container number: {c}")
        };
    }
}
