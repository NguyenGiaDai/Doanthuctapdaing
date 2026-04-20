using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Depot;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IDepotService
{
    Task<Pagination<DepotResponse>> GetAll(int pageNumber, int pageSize);
    Task<DepotResponse> Get(int id);
    Task<int> Create(CreateDepotRequest request);
    Task Update(int id, UpdateDepotRequest request);
}
