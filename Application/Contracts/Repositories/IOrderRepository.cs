using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }




}
