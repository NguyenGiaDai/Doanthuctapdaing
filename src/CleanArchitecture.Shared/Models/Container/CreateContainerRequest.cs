namespace CleanArchitecture.Shared.Models.Container;

public class CreateContainerRequest
{
    public string ContainerNumber { get; set; } = string.Empty;
    public int ContainerTypeId { get; set; }
    public int LineOperatorId { get; set; }
    public DateTime? DateOfManufacture { get; set; }
    public string ContainerOwner { get; set; } = string.Empty;
    public string ContainerCondition { get; set; } = string.Empty;
    public string? ContainerClassification { get; set; }
    public string? CurrentStatus { get; set; }
}
