using Application.Contracts.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            return await _context.Orders.AsNoTracking()
                .Include(o => o.user).Include(oi=>oi.OrderItems).ThenInclude(oi=>oi.Product).ThenInclude(p=>p.Images).Where(o=>o.UserId ==userId)
                
                .OrderByDescending(o => o.OrderDate).ToListAsync();
        }
        
        //get the order  by specific id 

        public async Task<Order?>  GetByIdAsync(int id)
        {
            return await _context.Orders.Include(o => o.user).Include(o=>o.OrderItems)
                .ThenInclude(oi => oi.Product).ThenInclude(oi=>oi.Images).FirstOrDefaultAsync(i => i.Id == id);
        }

        //get the orders from the user by admin 

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.AsNoTracking()
        .Include(o => o.user).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(oi=>oi.Images)
          
                 
        .OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        //add the status to the order 

        public async Task UpdateAsync(Order order)
        {
         
            _context.Entry(order).State = EntityState.Modified;
         
            await Task.CompletedTask;
        }

        public async Task<bool> AnyAsync(Expression<Func<Order, bool>> predicate)
        {
           
            return await _context.Orders.AnyAsync(predicate);
        }

        public async Task<int> AddReturnRequestAsync(ReturnRequest returnRequest)
        {
            await _context.ReturnRequests.AddAsync(returnRequest);
            await _context.SaveChangesAsync();
            return returnRequest.Id;
        }

        public async Task<List<ReturnRequest>> GetAllReturnRequestsAsync()
        {
           
            return await _context.ReturnRequests
                .Include(r => r.Product)
                .Include(r => r.Order)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

    }
}
