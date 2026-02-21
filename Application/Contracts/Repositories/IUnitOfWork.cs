using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }

        IProductRepository Products { get; }

        IUserRepository Users { get; }
        //Task SaveChangesAsync();
        //Task BeginTransactionAsync();
        //Task CommitTransactionAsync();
        //Task RollbackTransactionAsync();

        IExecutionStrategy CreateExecutionStrategy();

        Task ExecuteAsync(Func<Task> operation);
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation);


    }
}
