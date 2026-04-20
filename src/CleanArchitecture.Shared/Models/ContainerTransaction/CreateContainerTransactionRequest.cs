namespace CleanArchitecture.Shared.Models.ContainerTransaction;

public class CreateContainerTransactionRequest
{
    public int ContainerId { get; set; }
    public string TransactionType { get; set; } = string.Empty;

    public int? FromBlockId { get; set; }
    public int? FromBay { get; set; }
    public int? FromRow { get; set; }
    public int? FromTier { get; set; }

    public int? ToBlockId { get; set; }
    public int? ToBay { get; set; }
    public int? ToRow { get; set; }
    public int? ToTier { get; set; }

    public string? VehicleNumber { get; set; }
    public DateTime? TransactionTime { get; set; }
    public string? Note { get; set; }
}
