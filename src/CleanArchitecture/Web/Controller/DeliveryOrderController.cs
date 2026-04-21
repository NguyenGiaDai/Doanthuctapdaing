using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models.DeliveryOrder;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class DeliveryOrderController(IDeliveryOrderService deliveryOrderService) : BaseController
{
    private readonly IDeliveryOrderService _deliveryOrderService = deliveryOrderService;

    /// <summary>
    /// Get all delivery orders
    /// </summary>
    [HttpGet]
    [SwaggerResponse(200, "Delivery order list retrieved successfully.")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        => Ok(await _deliveryOrderService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// Get delivery order by id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Delivery order details retrieved successfully.", typeof(DeliveryOrderResponse))]
    [SwaggerResponse(404, "Delivery order not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _deliveryOrderService.Get(id));

    /// <summary>
    /// Create delivery order
    /// </summary>
    [HttpPost]
    [SwaggerResponse(200, "Delivery order created successfully.")]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryOrderRequest request)
        => Ok(await _deliveryOrderService.Create(request));

    /// <summary>
    /// Update delivery order
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Delivery order updated successfully.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeliveryOrderRequest request)
    {
        await _deliveryOrderService.Update(id, request);
        return Ok();
    }
}
