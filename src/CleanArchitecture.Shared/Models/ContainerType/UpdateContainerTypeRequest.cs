namespace CleanArchitecture.Shared.Models.ContainerType;

public class UpdateContainerTypeRequest
{
    public string ContainerTypeCode { get; set; } = string.Empty;
    public string ContainerTypeName { get; set; } = string.Empty;
    public string ISOCode { get; set; } = string.Empty;
    public int ContainerSize { get; set; }
    public decimal? MaximumWeight { get; set; }
    public decimal? TareWeight { get; set; }
}
