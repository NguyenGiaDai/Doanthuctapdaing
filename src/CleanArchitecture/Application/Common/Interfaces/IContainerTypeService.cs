using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerType;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IContainerTypeService
{
    Task<Pagination<ContainerTypeResponse>> GetAll(int pageNumber, int pageSize);
    Task<ContainerTypeResponse> Get(int id);
    Task<int> Create(CreateContainerTypeRequest request);
    Task Update(int id, UpdateContainerTypeRequest request);
}
