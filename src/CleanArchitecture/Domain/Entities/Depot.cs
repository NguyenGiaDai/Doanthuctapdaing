namespace CleanArchitecture.Domain.Entities;

public class Depot
{
    public int Id { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ICollection<Block> Blocks { get; set; } = new List<Block>();
}
