using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.DeliveryOrder;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IDeliveryOrderService
{
    Task<Pagination<DeliveryOrderResponse>> GetAll(int pageNumber, int pageSize);
    Task<DeliveryOrderResponse> Get(int id);
    Task<int> Create(CreateDeliveryOrderRequest request);
    Task Update(int id, UpdateDeliveryOrderRequest request);
}
