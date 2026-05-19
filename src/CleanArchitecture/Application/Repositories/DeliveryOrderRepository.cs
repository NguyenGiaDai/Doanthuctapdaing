using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application.Repositories;

public class DeliveryOrderRepository(ApplicationDbContext context)
    : GenericRepository<DeliveryOrder>(context), IDeliveryOrderRepository
{
}
