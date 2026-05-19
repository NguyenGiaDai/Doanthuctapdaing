using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class LineOperatorRepository(ApplicationDbContext context)
    : GenericRepository<LineOperator>(context), ILineOperatorRepository
{
}
