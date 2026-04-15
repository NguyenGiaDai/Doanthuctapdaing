namespace CleanArchitecture.Shared.Models.Container;

public class ContainerResponse
{
    public int Id { get; set; }
    public string ContainerNumber { get; set; } = string.Empty;
    public string ContainerType { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public string ContainerSize { get; set; } = string.Empty;
    public decimal? MaximumWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public DateTime? DateOfManufacture { get; set; }
    public string ContainerOwner { get; set; } = string.Empty;
    public string ContainerCondition { get; set; } = string.Empty;
}
