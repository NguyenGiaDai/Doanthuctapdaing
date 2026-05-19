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
    /// import a container into depot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("import")]
    [SwaggerResponse(200, "Container imported successfully.", typeof(int))]
    public async Task<IActionResult> ImportContainer([FromBody] ImportContainerRequest request)
        => Ok(await _containerTransactionService.ImportContainer(request));
    
    /// <summary>
    /// export a container out of depot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("export")]
    [SwaggerResponse(200, "Container exported successfully.", typeof(int))]
    public async Task<IActionResult> ExportContainer([FromBody] ExportContainerRequest request)
        => Ok(await _containerTransactionService.ExportContainer(request));
    
    /// <summary>
    /// report import/export throughput by line operator by date
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("report/throughput-by-line-operator")]
    [SwaggerResponse(200, "Container throughput report retrieved successfully.", typeof(List<ContainerThroughputReportResponse>))]
    public async Task<IActionResult> GetContainerThroughputReport([FromBody] ContainerThroughputReportRequest request)
        => Ok(await _containerTransactionService.GetContainerThroughputReport(request));
    
    /// <summary>
    /// report container inventory in yard by line operator and storage time
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("report/yard-inventory-by-line-operator")]
    [SwaggerResponse(200, "Container yard inventory report retrieved successfully.", typeof(List<ContainerYardInventoryReportResponse>))]
    public async Task<IActionResult> GetContainerYardInventoryReport([FromBody] ContainerYardInventoryReportRequest request)
        => Ok(await _containerTransactionService.GetContainerYardInventoryReport(request));

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
