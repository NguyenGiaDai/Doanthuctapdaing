using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class ContainerTypeRepository(ApplicationDbContext context)
    : GenericRepository<ContainerType>(context), IContainerTypeRepository
{
}
