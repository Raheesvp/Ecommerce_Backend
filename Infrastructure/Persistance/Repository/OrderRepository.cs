using Application.Contracts.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repository
{
    public  class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }
        //get the orders and write the implementatition of interface
        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders.Where(o => o.UserId == userId)
                .Include(o => o.OrderItems).ThenInclude(oi=>oi.Product)
                
                .OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        //get the order  by specific id 

        public async Task<Order?>  GetByIdAsync(int id)
        {
            return await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(i => i.Id == id);
        }


    }
}
