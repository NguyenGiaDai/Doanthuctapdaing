namespace CleanArchitecture.Domain.Entities;

public class Container
{
    public int Id { get; set; }
    public string ContainerNumber { get; set; } = string.Empty;

    public int ContainerTypeId { get; set; }
    public int LineOperatorId { get; set; }

    public DateTime? DateOfManufacture { get; set; }
    public string ContainerOwner { get; set; } = string.Empty;
    public string ContainerCondition { get; set; } = string.Empty;
    public string? ContainerClassification { get; set; }
    public string? CurrentStatus { get; set; }

    public ContainerType? ContainerTypeNavigation { get; set; }
    public LineOperator? LineOperator { get; set; }

    public ICollection<ContainerPosition> ContainerPositions { get; set; } = new List<ContainerPosition>();
    public ICollection<ContainerTransaction> ContainerTransactions { get; set; } = new List<ContainerTransaction>();
}
