namespace CleanArchitecture.Shared.Models.Block;

public class BlockResponse
{
    public int Id { get; set; }
    public int DepotId { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string BlockCode { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;
    public string BlockType { get; set; } = string.Empty;
    public int? MaxBay { get; set; }
    public int? MaxRow { get; set; }
    public int? MaxTier { get; set; }
}
