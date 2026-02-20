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

        public async Task ExecuteAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    await action();
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

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
