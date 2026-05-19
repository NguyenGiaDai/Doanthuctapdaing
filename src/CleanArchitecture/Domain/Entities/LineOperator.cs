namespace CleanArchitecture.Domain.Entities;

public class LineOperator
{
    public int Id { get; set; }
    public string LineOperatorCode { get; set; } = string.Empty;
    public string LineOperatorName { get; set; } = string.Empty;

    public ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();
    public ICollection<Container> Containers { get; set; } = new List<Container>();
}
