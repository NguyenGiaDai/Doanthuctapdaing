using AutoMapper;
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
            throw new Exception("Customer not found");

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<int> Create(CreateCustomerRequest request)
    {
        var isExist = await _unitOfWork.CustomerRepository.AnyAsync(x => x.CustomerCode == request.CustomerCode);

        if (isExist)
            throw new Exception("Customer code already exists");

        var customer = _mapper.Map<Customer>(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
            await _unitOfWork.CustomerRepository.AddAsync(customer), CancellationToken.None);

        return customer.Id;
    }

    public async Task Update(int id, UpdateCustomerRequest request)
    {
        var customer = await _unitOfWork.CustomerRepository.FirstOrDefaultAsync(x => x.Id == id);

        if (customer == null)
            throw new Exception("Customer not found");

        var isDuplicate = await _unitOfWork.CustomerRepository.AnyAsync(x =>
            x.Id != id && x.CustomerCode == request.CustomerCode);

        if (isDuplicate)
            throw new Exception("Customer code already exists");

        customer.CustomerCode = request.CustomerCode;
        customer.CustomerName = request.CustomerName;
        customer.CustomerTaxCode = request.CustomerTaxCode;
        customer.Address = request.Address;

        await _unitOfWork.ExecuteTransactionAsync(() =>
        {
            _unitOfWork.CustomerRepository.Update(customer);
        }, CancellationToken.None);
    }
}
