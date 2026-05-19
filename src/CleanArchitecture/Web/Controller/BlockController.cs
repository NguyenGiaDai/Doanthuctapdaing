using CleanArchitecture.Application;
using CleanArchitecture.Shared.Models.Block;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Web.Controller;

public class BlockController(IBlockService blockService) : BaseController
{
    private readonly IBlockService _blockService = blockService;

    /// <summary>
    /// get a list of blocks
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerResponse(200, "Block list retrieved successfully.")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        => Ok(await _blockService.GetAll(pageNumber, pageSize));

    /// <summary>
    /// get a block by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Block details retrieved successfully.", typeof(BlockResponse))]
    [SwaggerResponse(404, "Block not found.")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _blockService.Get(id));

    /// <summary>
    /// create a new block
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(200, "Block created successfully.")]
    [SwaggerResponse(400, "Invalid request.")]
    public async Task<IActionResult> Create([FromBody] CreateBlockRequest request)
        => Ok(await _blockService.Create(request));

    /// <summary>
    /// update a block
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SwaggerResponse(200, "Block updated successfully.")]
    [SwaggerResponse(400, "Invalid request.")]
    [SwaggerResponse(404, "Block not found.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlockRequest request)
    {
        await _blockService.Update(id, request);
        return Ok();
    }
}
