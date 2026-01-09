using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repositories
{
    public  interface ICategoryRepository
    {
        Task<List<CategoryEntity>> GetAllAsync();
        Task<CategoryEntity> GetByNameAsync(string name);
        Task<CategoryEntity> AddAsync(CategoryEntity category);
        Task UpdateAsync(CategoryEntity category);

        Task<CategoryEntity?> GetByIdAsync(int id);
        Task DeleteAsync(CategoryEntity category);
    }
}
