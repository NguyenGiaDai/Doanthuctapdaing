using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models.Depot;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class DepotController(IDepotService depotService) : BaseController
{
    private readonly IDepotService _depotService = depotService;

    /// <summary>
    /// get a list of depots
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerResponse(200, "Depot list retrieved successfully.")]
    public async Task<IActionResult> Get([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        => Ok(await _depotService.GetAll(pageIndex, pageSize));

    /// <summary>
    /// get a depot by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Depot details retrieved successfully.", typeof(DepotResponse))]
    [SwaggerResponse(404, "Depot not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _depotService.Get(id));

    /// <summary>
    /// create a new depot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(200, "Depot created successfully.")]
    public async Task<IActionResult> Post([FromBody] CreateDepotRequest request)
        => Ok(await _depotService.Create(request));

    /// <summary>
    /// update depot
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Depot updated successfully.")]
    [SwaggerResponse(404, "Depot not found.")]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateDepotRequest request)
    {
        await _depotService.Update(id, request);
        return Ok();
    }
}
