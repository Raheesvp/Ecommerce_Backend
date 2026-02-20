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

        Task ExecuteAsync(Func<Task> action);


    }
}
