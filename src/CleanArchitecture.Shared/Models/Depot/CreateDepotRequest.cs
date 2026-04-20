namespace CleanArchitecture.Shared.Models.Depot;

public class CreateDepotRequest
{
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string? Address { get; set; }
}
