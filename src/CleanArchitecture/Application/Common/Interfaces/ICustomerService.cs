using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Customer;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface ICustomerService
{
    Task<Pagination<CustomerResponse>> GetAll(int pageNumber, int pageSize);
    Task<CustomerResponse> Get(int id);
    Task<int> Create(CreateCustomerRequest request);
    Task Update(int id, UpdateCustomerRequest request);
}
