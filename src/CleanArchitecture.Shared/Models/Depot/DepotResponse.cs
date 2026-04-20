namespace CleanArchitecture.Shared.Models.Depot;

public class DepotResponse
{
    public int Id { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string? Address { get; set; }
}
