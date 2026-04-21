using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models.Customer;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class CustomerController(ICustomerService customerService) : BaseController
{
    private readonly ICustomerService _customerService = customerService;

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    [SwaggerResponse(200, "Customer list retrieved successfully.")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        => Ok(await _customerService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// Get customer by id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Customer details retrieved successfully.", typeof(CustomerResponse))]
    [SwaggerResponse(404, "Customer not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _customerService.Get(id));

    /// <summary>
    /// Create customer
    /// </summary>
    [HttpPost]
    [SwaggerResponse(200, "Customer created successfully.")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        => Ok(await _customerService.Create(request));

    /// <summary>
    /// Update customer
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Customer updated successfully.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        await _customerService.Update(id, request);
        return Ok();
    }
}
