namespace CleanArchitecture.Shared.Models.ContainerTransaction;

public class ContainerYardInventoryReportResponse
{
    public int? LineOperatorId { get; set; }
    public string LineOperatorCode { get; set; } = string.Empty;
    public string LineOperatorName { get; set; } = string.Empty;

    public int From0To10DaysCount { get; set; }
    public int From10DaysOrMoreCount { get; set; }
    public int TotalInYardCount { get; set; }
}
