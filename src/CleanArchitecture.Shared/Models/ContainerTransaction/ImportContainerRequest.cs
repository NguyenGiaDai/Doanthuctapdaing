namespace CleanArchitecture.Shared.Models.ContainerTransaction;

public class ImportContainerRequest
{
    public int ContainerId { get; set; }

    public int ToBlockId { get; set; }
    public int ToBay { get; set; }
    public int ToRow { get; set; }
    public int ToTier { get; set; }

    public string? VehicleNumber { get; set; }
    public DateTime? TransactionTime { get; set; }
    public string? Note { get; set; }
}
