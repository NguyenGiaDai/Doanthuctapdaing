using CleanArchitecture.Infrastructure.Interface;

namespace CleanArchitecture.Application;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IBookRepository BookRepository { get; }
    IContainerRepository ContainerRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IMediaRepository MediaRepository { get; }
    IForgotPasswordRepository ForgotPasswordRepository { get; }
    Task SaveChangesAsync(CancellationToken token);
    Task ExecuteTransactionAsync(Action action, CancellationToken token);
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken token);
}
