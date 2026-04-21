namespace CleanArchitecture.Shared.Models.DeliveryOrder;

public class CreateDeliveryOrderRequest
{
    public string DONumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int LineOperatorId { get; set; }
    public int ContainerTypeId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? VesselVoyage { get; set; }
    public string? OrderStatus { get; set; }
}
