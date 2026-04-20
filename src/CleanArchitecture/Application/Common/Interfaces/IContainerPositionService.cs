using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerPosition;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IContainerPositionService
{
    Task<Pagination<ContainerPositionResponse>> GetAll(int pageNumber, int pageSize);
    Task<ContainerPositionResponse> Get(int id);
    Task<int> Create(CreateContainerPositionRequest request);
    Task Update(int id, UpdateContainerPositionRequest request);
}
