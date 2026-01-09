using Application.Contracts.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryEntity>> GetAllAsync()
        {
            
            return await _context.CategoryEntities.ToListAsync();
        }

        public async Task<CategoryEntity?> GetByIdAsync(int id)
        {
            return await _context.CategoryEntities.FindAsync(id);
        }

        public async Task<CategoryEntity?> GetByNameAsync(string name)
        {
            
            return await _context.CategoryEntities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<CategoryEntity> AddAsync(CategoryEntity category)
        {
            await _context.CategoryEntities.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(CategoryEntity category)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(CategoryEntity category)
        {
            _context.CategoryEntities.Remove(category);
            await _context.SaveChangesAsync();
        }

    }
}

