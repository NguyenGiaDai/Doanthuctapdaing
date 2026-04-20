namespace CleanArchitecture.Shared.Models.ContainerPosition;

public class ContainerPositionResponse
{
    public int Id { get; set; }

    public int ContainerId { get; set; }
    public int BlockId { get; set; }

    public int Bay { get; set; }
    public int Row { get; set; }
    public int Tier { get; set; }

    public DateTime? PositionTime { get; set; }
}
