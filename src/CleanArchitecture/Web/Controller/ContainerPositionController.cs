using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models.ContainerPosition;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class ContainerPositionController(IContainerPositionService containerPositionService) : BaseController
{
    private readonly IContainerPositionService _containerPositionService = containerPositionService;

    /// <summary>
    /// get a list of container positions
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerResponse(200, "Container position list retrieved successfully.")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        => Ok(await _containerPositionService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// get a container position by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Container position details retrieved successfully.", typeof(ContainerPositionResponse))]
    [SwaggerResponse(404, "Container position not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _containerPositionService.Get(id));

    /// <summary>
    /// create a new container position
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(200, "Container position created successfully.")]
    [SwaggerResponse(400, "Invalid request.")]
    public async Task<IActionResult> Create([FromBody] CreateContainerPositionRequest request)
        => Ok(await _containerPositionService.Create(request));

    /// <summary>
    /// update a container position
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Container position updated successfully.")]
    [SwaggerResponse(400, "Invalid request.")]
    [SwaggerResponse(404, "Container position not found.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContainerPositionRequest request)
    {
        await _containerPositionService.Update(id, request);
        return Ok();
    }
}
