using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class BlockRepository(ApplicationDbContext context)
    : GenericRepository<Block>(context), IBlockRepository
{
}
