namespace CleanArchitecture.Shared.Models.ContainerTransaction;

public class ExportContainerRequest
{
    public int ContainerId { get; set; }
    public int DeliveryOrderId { get; set; }

    public string? VehicleNumber { get; set; }
    public DateTime? TransactionTime { get; set; }
    public string? Note { get; set; }
}
