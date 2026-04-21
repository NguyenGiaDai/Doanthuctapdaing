using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models.LineOperator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class LineOperatorController(ILineOperatorService lineOperatorService) : BaseController
{
    private readonly ILineOperatorService _lineOperatorService = lineOperatorService;

    /// <summary>
    /// Get all line operators
    /// </summary>
    [HttpGet]
    [SwaggerResponse(200, "Line operator list retrieved successfully.")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        => Ok(await _lineOperatorService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// Get line operator by id
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Line operator details retrieved successfully.", typeof(LineOperatorResponse))]
    [SwaggerResponse(404, "Line operator not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _lineOperatorService.Get(id));

    /// <summary>
    /// Create line operator
    /// </summary>
    [HttpPost]
    [SwaggerResponse(200, "Line operator created successfully.")]
    public async Task<IActionResult> Create([FromBody] CreateLineOperatorRequest request)
        => Ok(await _lineOperatorService.Create(request));

    /// <summary>
    /// Update line operator
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Line operator updated successfully.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLineOperatorRequest request)
    {
        await _lineOperatorService.Update(id, request);
        return Ok();
    }
}
