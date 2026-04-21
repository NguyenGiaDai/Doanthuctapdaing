namespace CleanArchitecture.Domain.Entities;

public class DeliveryOrder
{
    public int Id { get; set; }

    public string DONumber { get; set; } = string.Empty;

    public int ContainerTypeId { get; set; }
    public int LineOperatorId { get; set; }
    public int CustomerId { get; set; }

    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? VesselVoyage { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? OrderStatus { get; set; }

    public ContainerType? ContainerType { get; set; }
    public LineOperator? LineOperator { get; set; }
    public Customer? Customer { get; set; }
}
