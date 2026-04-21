using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models.ContainerType;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class ContainerTypeController(IContainerTypeService containerTypeService) : BaseController
{
    private readonly IContainerTypeService _containerTypeService = containerTypeService;

    /// <summary>
    /// Get all container types
    /// </summary>
    [HttpGet]
    [SwaggerResponse(200, "Container type list retrieved successfully.")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        => Ok(await _containerTypeService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// Get container type by id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Container type details retrieved successfully.", typeof(ContainerTypeResponse))]
    [SwaggerResponse(404, "Container type not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _containerTypeService.Get(id));

    /// <summary>
    /// Create container type
    /// </summary>
    [HttpPost]
    [SwaggerResponse(200, "Container type created successfully.")]
    public async Task<IActionResult> Create([FromBody] CreateContainerTypeRequest request)
        => Ok(await _containerTypeService.Create(request));

    /// <summary>
    /// Update container type
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Container type updated successfully.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContainerTypeRequest request)
    {
        await _containerTypeService.Update(id, request);
        return Ok();
    }
}
