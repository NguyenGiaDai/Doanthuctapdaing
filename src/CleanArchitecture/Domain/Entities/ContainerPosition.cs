namespace CleanArchitecture.Domain.Entities;

public class ContainerPosition
{
    public int Id { get; set; }

    public int ContainerId { get; set; }
    public int BlockId { get; set; }

    public int Bay { get; set; }
    public int Row { get; set; }
    public int Tier { get; set; }

    public DateTime? PositionTime { get; set; }

    public Container? Container { get; set; }
    public Block? Block { get; set; }
}
