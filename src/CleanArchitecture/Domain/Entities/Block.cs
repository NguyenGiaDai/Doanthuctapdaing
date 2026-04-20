namespace CleanArchitecture.Domain.Entities;

public class Block
{
    public int Id { get; set; }
    public int DepotId { get; set; }

    public string BlockCode { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;

    // Real / Virtual
    public string BlockType { get; set; } = string.Empty;

    // Với block ảo có thể để null
    public int? MaxBay { get; set; }
    public int? MaxRow { get; set; }
    public int? MaxTier { get; set; }

    public Depot? Depot { get; set; }

    public ICollection<ContainerPosition> ContainerPositions { get; set; } = new List<ContainerPosition>();
    public ICollection<ContainerTransaction> FromContainerTransactions { get; set; } = new List<ContainerTransaction>();
    public ICollection<ContainerTransaction> ToContainerTransactions { get; set; } = new List<ContainerTransaction>();
}
