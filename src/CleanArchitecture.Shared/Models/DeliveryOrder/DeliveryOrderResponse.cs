namespace CleanArchitecture.Shared.Models.DeliveryOrder;

public class DeliveryOrderResponse
{
    public int Id { get; set; }
    public string DONumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public int LineOperatorId { get; set; }
    public string? LineOperatorName { get; set; }

    public int ContainerTypeId { get; set; }
    public string? ContainerTypeName { get; set; }

    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? VesselVoyage { get; set; }
    public string? OrderStatus { get; set; }
}
