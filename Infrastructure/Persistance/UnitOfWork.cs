using Application.Contracts.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IOrderRepository Orders { get; }
        public IProductRepository Products { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(
            AppDbContext context,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository)
        {
            _context = context;
            Orders = orderRepository;
            Products = productRepository;
            Users = userRepository;
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    await operation();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            T result = default!;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database
                    .BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                try
                {
                    result = await operation();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return result;
        }


        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
