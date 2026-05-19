using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Block;

namespace CleanArchitecture.Application;

public interface IBlockService
{
    Task<Pagination<BlockResponse>> GetAll(int pageNumber, int pageSize);
    Task<BlockResponse> Get(int id);
    Task<int> Create(CreateBlockRequest request);
    Task Update(int id, UpdateBlockRequest request);
}
