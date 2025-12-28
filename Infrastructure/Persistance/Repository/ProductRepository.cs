    using Application.Contracts.Repositories;
    using Domain.Entities;
    using Infrastructure.Persistance.Repository;
    using Infrastructure.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Identity.Client;

    namespace Infrastructure.Persistence.Repository
    {
        public class ProductRepository : GenericeRepository<Product> ,IProductRepository
        {
            private readonly AppDbContext _context;

            public ProductRepository(AppDbContext context ):base (context)
            {
                _context = context;
            }

            public async Task<Product?> GetByIdAsync(int id) =>
                await _context.Products.Include(p=>p.Images).FirstOrDefaultAsync(p => p.Id == id);

            public async Task<IReadOnlyList<Product>> GetAllAsync() =>
                await _context.Products
                    .AsNoTracking().Include(p=>p.Images)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

            public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category) =>
                await _context.Products
                    .AsNoTracking().Include(p=>p.Images)
                    .Where(p => p.Category == category)
                    .ToListAsync();

            public async Task<IReadOnlyList<Product>> SearchAsync(string query) =>
                await _context.Products
                    .AsNoTracking().Include(p=>p.Images)
                    .Where(p => p.Name.Contains(query) || p.Category.Contains(query))
                    .ToListAsync();

            public async Task<IReadOnlyList<Product>> GetActivePaginatedAsync(int pageNumber, int pageSize) =>
                await _context.Products
                    .AsNoTracking().Include(p=>p.Images).Where(p=>p.IsActive == true).OrderBy(p=>p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            public async Task<int> CountAsync() =>
                await _context.Products.CountAsync();

            public async Task<bool> ExistByNameAsync(string name) =>
                await _context.Products.AnyAsync(p => p.Name == name);

            //public async Task AddAsync(Product product)
            //{
            //    _context.Products.Add(product);
            //    await _context.SaveChangesAsync();
            //}

            //public async Task UpdateAsync(Product product)
            //{
            //    _context.Products.Update(product);
            //    await _context.SaveChangesAsync();
            //}

            //public async Task DeleteAsync(Product product)
            //{
            //    _context.Products.Remove(product);
            //    await _context.SaveChangesAsync();
            //}

            public async Task<bool> ExistsAsync(int productId)
            {
                // The Query Filter automatically ignores deleted products
                return await _context.Products.AnyAsync(p => p.Id == productId);
            }

            public async Task<IReadOnlyList<Product>> GetFeaturedAsync()
            {
                return await _context.Products.Where(p => p.Featured).Include(p => p.Images).ToListAsync();
            }

        public async Task<int> CountActiveAsync()
        {
            // Only count products where IsActive is true
            return await _context.Products.CountAsync(p => p.IsActive == true);
        }

    }
    }
