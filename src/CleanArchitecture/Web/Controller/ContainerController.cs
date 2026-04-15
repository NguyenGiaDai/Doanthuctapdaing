using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Container;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class ContainerController(IContainerService containerService) : BaseController
{
    private readonly IContainerService _containerService = containerService;

    /// <summary>
    /// get a container by id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Container details retrieved successfully.", typeof(ContainerResponse))]
    [SwaggerResponse(404, "Container not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _containerService.Get(id));

    /// <summary>
    /// get a list of containers
    /// </summary>
    [HttpGet]
    [SwaggerResponse(200, "Containers retrieved successfully.", typeof(Pagination<ContainerResponse>))]
    public async Task<IActionResult> Get(int pageIndex = 0, int pageSize = 10)
        => Ok(await _containerService.Get(pageIndex, pageSize));

    /// <summary>
    /// add a container
    /// </summary>
    [HttpPost]
    [SwaggerResponse(201, "Container added successfully.", typeof(ContainerResponse))]
    [SwaggerResponse(400, "Invalid request.")]
    public async Task<IActionResult> Add(CreateContainerRequest request, CancellationToken token)
        => Ok(await _containerService.Add(request, token));

    /// <summary>
    /// update a container
    /// </summary>
    [HttpPut]
    [SwaggerResponse(200, "Container updated successfully.", typeof(ContainerResponse))]
    [SwaggerResponse(404, "Container not found.")]
    public async Task<IActionResult> Update(UpdateContainerRequest request, CancellationToken token)
        => Ok(await _containerService.Update(request, token));
}
