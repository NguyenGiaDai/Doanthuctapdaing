using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Container;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IContainerService
{
    Task<Pagination<ContainerResponse>> Get(int pageIndex, int pageSize);
    Task<ContainerResponse> Get(int id);
    Task<ContainerResponse> Add(CreateContainerRequest request, CancellationToken token);
    Task<ContainerResponse> Update(UpdateContainerRequest request, CancellationToken token);
}
