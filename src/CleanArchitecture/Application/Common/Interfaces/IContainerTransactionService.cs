using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerTransaction;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IContainerTransactionService
{
    Task<Pagination<ContainerTransactionResponse>> GetAll(int pageNumber, int pageSize);
    Task<ContainerTransactionResponse> Get(int id);
    Task<int> Create(CreateContainerTransactionRequest request);
    Task Update(int id, UpdateContainerTransactionRequest request);
}
