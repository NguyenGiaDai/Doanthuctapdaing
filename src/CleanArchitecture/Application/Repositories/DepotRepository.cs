using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class DepotRepository : GenericRepository<Depot>, IDepotRepository
{
    public DepotRepository(ApplicationDbContext context) : base(context)
    {
    }
}
