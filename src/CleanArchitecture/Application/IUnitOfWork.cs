using CleanArchitecture.Infrastructure.Interface;
namespace CleanArchitecture.Application;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IContainerRepository ContainerRepository { get; }
    IBlockRepository BlockRepository { get; }
    IDepotRepository DepotRepository { get; }
    IContainerPositionRepository ContainerPositionRepository { get; }
    IContainerTransactionRepository ContainerTransactionRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IMediaRepository MediaRepository { get; }
    Task SaveChangesAsync(CancellationToken token);
    Task ExecuteTransactionAsync(Action action, CancellationToken token);
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken token);
}
