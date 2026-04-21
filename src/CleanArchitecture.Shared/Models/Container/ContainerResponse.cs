namespace CleanArchitecture.Shared.Models.Container;

public class ContainerResponse
{
    public int Id { get; set; }
    public string ContainerNumber { get; set; } = string.Empty;

    public int ContainerTypeId { get; set; }
    public string ContainerTypeCode { get; set; } = string.Empty;
    public string ContainerTypeName { get; set; } = string.Empty;
    public string ISOCode { get; set; } = string.Empty;
    public int ContainerSize { get; set; }
    public decimal? MaximumWeight { get; set; }
    public decimal? TareWeight { get; set; }

    public int LineOperatorId { get; set; }
    public string LineOperatorCode { get; set; } = string.Empty;
    public string LineOperatorName { get; set; } = string.Empty;

    public DateTime? DateOfManufacture { get; set; }
    public string ContainerOwner { get; set; } = string.Empty;
    public string ContainerCondition { get; set; } = string.Empty;
    public string? ContainerClassification { get; set; }
    public string? CurrentStatus { get; set; }
}
