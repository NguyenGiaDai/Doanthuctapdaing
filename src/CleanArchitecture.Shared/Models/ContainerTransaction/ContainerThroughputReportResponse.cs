namespace CleanArchitecture.Shared.Models.ContainerTransaction;

public class ContainerThroughputReportResponse
{
    public int? LineOperatorId { get; set; }
    public string LineOperatorCode { get; set; } = string.Empty;
    public string LineOperatorName { get; set; } = string.Empty;

    public int ImportCount { get; set; }
    public int ExportCount { get; set; }
    public int TotalCount { get; set; }
}
