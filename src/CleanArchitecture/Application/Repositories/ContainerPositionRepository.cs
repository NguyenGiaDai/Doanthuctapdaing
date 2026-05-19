using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class ContainerPositionRepository(ApplicationDbContext context)
    : GenericRepository<ContainerPosition>(context), IContainerPositionRepository
{
}
