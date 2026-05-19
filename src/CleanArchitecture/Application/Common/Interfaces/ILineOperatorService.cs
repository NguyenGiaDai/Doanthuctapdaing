using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.LineOperator;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface ILineOperatorService
{
    Task<Pagination<LineOperatorResponse>> GetAll(int pageNumber, int pageSize);
    Task<LineOperatorResponse> Get(int id);
    Task<int> Create(CreateLineOperatorRequest request);
    Task Update(int id, UpdateLineOperatorRequest request);
}
