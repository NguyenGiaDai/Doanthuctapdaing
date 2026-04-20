using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.ContainerTransaction;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class ContainerTransactionController(IContainerTransactionService containerTransactionService) : BaseController
{
    private readonly IContainerTransactionService _containerTransactionService = containerTransactionService;

    /// <summary>
    /// get a list of container transactions
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerResponse(200, "Container transaction list retrieved successfully.", typeof(Pagination<ContainerTransactionResponse>))]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        => Ok(await _containerTransactionService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// get a container transaction by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Container transaction retrieved successfully.", typeof(ContainerTransactionResponse))]
    [SwaggerResponse(404, "Container transaction not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _containerTransactionService.Get(id));

    /// <summary>
    /// create a new container transaction
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(200, "Container transaction created successfully.", typeof(int))]
    public async Task<IActionResult> Create([FromBody] CreateContainerTransactionRequest request)
        => Ok(await _containerTransactionService.Create(request));

    /// <summary>
    /// update a container transaction
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Container transaction updated successfully.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContainerTransactionRequest request)
    {
        await _containerTransactionService.Update(id, request);
        return Ok();
    }
}
