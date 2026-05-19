using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Customer;

namespace CleanArchitecture.Application.Services;

public class CustomerService(IUnitOfWork unitOfWork, IMapper mapper) : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<CustomerResponse>> GetAll(int pageNumber, int pageSize)
    {
        return await _unitOfWork.CustomerRepository.ToPagination(
            pageIndex: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.CustomerCode,
            ascending: true,
            selector: x => new CustomerResponse
            {
                Id = x.Id,
                CustomerCode = x.CustomerCode,
                CustomerName = x.CustomerName,
                CustomerTaxCode = x.CustomerTaxCode,
                Address = x.Address
            });
    }

    public async Task<CustomerResponse> Get(int id)
    {
        var customer = await _unitOfWork.CustomerRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (customer == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Customer not found");

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<int> Create(CreateCustomerRequest request)
    {
        ValidateRequest(request.CustomerCode, request.CustomerName, request.Address);

        var normalizedCode = request.CustomerCode.Trim().ToUpper();
        var normalizedName = request.CustomerName.Trim();
        var normalizedTaxCode = request.CustomerTaxCode?.Trim();
        var normalizedAddress = request.Address.Trim();

        var isExist = await _unitOfWork.CustomerRepository.AnyAsync(x => x.CustomerCode == normalizedCode);

        if (isExist)
            throw new UserFriendlyException(ErrorCode.Conflict, "Customer code already exists");

        var customer = _mapper.Map<Customer>(request);
        customer.CustomerCode = normalizedCode;
        customer.CustomerName = normalizedName;
        customer.CustomerTaxCode = normalizedTaxCode;
        customer.Address = normalizedAddress;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.CustomerRepository.AddAsync(customer), CancellationToken.None);

        return customer.Id;
    }

    public async Task Update(int id, UpdateCustomerRequest request)
    {
        ValidateRequest(request.CustomerCode, request.CustomerName, request.Address);

        var customer = await _unitOfWork.CustomerRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (customer == null)
            throw new UserFriendlyException(ErrorCode.NotFound, "Customer not found");

        var normalizedCode = request.CustomerCode.Trim().ToUpper();
        var normalizedName = request.CustomerName.Trim();
        var normalizedTaxCode = request.CustomerTaxCode?.Trim();
        var normalizedAddress = request.Address.Trim();

        var isDuplicate = await _unitOfWork.CustomerRepository.AnyAsync(x =>
            x.Id != id && x.CustomerCode == normalizedCode);

        if (isDuplicate)
            throw new UserFriendlyException(ErrorCode.Conflict, "Customer code already exists");

        customer.CustomerCode = normalizedCode;
        customer.CustomerName = normalizedName;
        customer.CustomerTaxCode = normalizedTaxCode;
        customer.Address = normalizedAddress;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.CustomerRepository.Update(customer);
        }, CancellationToken.None);
    }

    private static void ValidateRequest(string customerCode, string customerName, string address)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Customer code is required");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Customer name is required");

        if (string.IsNullOrWhiteSpace(address))
            throw new UserFriendlyException(ErrorCode.BadRequest, "Address is required");
    }
}
