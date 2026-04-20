namespace CleanArchitecture.Domain.Entities;

public class ContainerTransaction
{
    public int Id { get; set; }

    public int ContainerId { get; set; }

    // In / Out / Move
    public string TransactionType { get; set; } = string.Empty;

    public DateTime TransactionTime { get; set; }

    public string? VehicleNumber { get; set; }

    public int? FromBlockId { get; set; }
    public int? FromBay { get; set; }
    public int? FromRow { get; set; }
    public int? FromTier { get; set; }

    public int? ToBlockId { get; set; }
    public int? ToBay { get; set; }
    public int? ToRow { get; set; }
    public int? ToTier { get; set; }

    public string? Note { get; set; }

    public Container? Container { get; set; }
    public Block? FromBlock { get; set; }
    public Block? ToBlock { get; set; }
}
