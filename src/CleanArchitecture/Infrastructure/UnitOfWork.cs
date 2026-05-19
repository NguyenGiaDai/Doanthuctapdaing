using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Repositories;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;


namespace CleanArchitecture.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IUserRepository UserRepository { get; }
    public IContainerRepository ContainerRepository { get; }
    public IDepotRepository DepotRepository { get; }
    public IBlockRepository BlockRepository { get; }
    public IContainerPositionRepository ContainerPositionRepository { get; }
    public IContainerTransactionRepository ContainerTransactionRepository { get; }
    public ICustomerRepository CustomerRepository { get; }
    public ILineOperatorRepository LineOperatorRepository { get; }
    public IContainerTypeRepository ContainerTypeRepository { get; }
    public IDeliveryOrderRepository DeliveryOrderRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }
    public IMediaRepository MediaRepository { get; }

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _context = dbContext;
        UserRepository = new UserRepository(_context);
        ContainerRepository = new ContainerRepository(_context);
        DepotRepository = new DepotRepository(_context);
        BlockRepository = new BlockRepository(_context);
        ContainerPositionRepository = new ContainerPositionRepository(_context);
        ContainerTransactionRepository = new ContainerTransactionRepository(_context);
        CustomerRepository = new CustomerRepository(_context);
        LineOperatorRepository = new LineOperatorRepository(_context);
        ContainerTypeRepository = new ContainerTypeRepository(_context);
        DeliveryOrderRepository = new DeliveryOrderRepository(_context);
        RefreshTokenRepository = new RefreshTokenRepository(_context);
        MediaRepository = new MediaRepository(_context);
    }

    public async Task SaveChangesAsync(CancellationToken token)
        => await _context.SaveChangesAsync(token);

    public async Task ExecuteTransactionAsync(Action action, CancellationToken token)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(token);
        try
        {
            action();
            await _context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            throw TransactionException.TransactionNotExecuteException(ex);
        }
    }

    public async Task ExecuteTransactionAsync(Func<Task> action, CancellationToken token)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(token);
        try
        {
            await action();
            await _context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            throw TransactionException.TransactionNotExecuteException(ex);
        }
    }
}
