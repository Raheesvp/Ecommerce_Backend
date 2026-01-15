using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface IOrderRepository
    {
        //creating the order
        Task<Order> CreateAsync(Order order);

        //getting the orders

        Task<List<Order>> GetUserOrdersAsync(int userId);

        //get the order by specific Id 

        Task<Order?> GetByIdAsync(int id);

        //admin can view the orders of the user

        Task UpdateAsync(Order order);

        Task<List<Order>> GetAllAsync();

        Task<bool> AnyAsync(Expression<Func<Order, bool>> predicate);
    }




}
