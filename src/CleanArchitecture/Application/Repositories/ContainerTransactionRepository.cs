using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class ContainerTransactionRepository(ApplicationDbContext dbContext)
    : GenericRepository<ContainerTransaction>(dbContext), IContainerTransactionRepository
{
}
