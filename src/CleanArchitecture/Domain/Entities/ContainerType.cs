namespace CleanArchitecture.Domain.Entities;

public class ContainerType
{
    public int Id { get; set; }
    public string ContainerTypeCode { get; set; } = string.Empty;
    public string ContainerTypeName { get; set; } = string.Empty;
    public string ISOCode { get; set; } = string.Empty;
    public int ContainerSize { get; set; }
    public decimal? MaximumWeight { get; set; }
    public decimal? TareWeight { get; set; }

    public ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();
    public ICollection<Container> Containers { get; set; } = new List<Container>();
}
