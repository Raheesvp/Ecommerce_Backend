    using Application.Contracts.Repositories;
using Application.DTOs.Product;
using Domain.Entities;
    using Infrastructure.Persistance.Repository;
    using Infrastructure.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Identity.Client;

    namespace Infrastructure.Persistence.Repository
    {
    public class ProductRepository : GenericeRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IReadOnlyList<Product>> GetAllAsync() =>
            await _context.Products.Where(p => p.IsActive)
                .AsNoTracking().Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category) =>
            await _context.Products
                .AsNoTracking().Include(p => p.Images)
                .Where(p => p.Category == category)
                .ToListAsync();

        public async Task<IReadOnlyList<Product>> SearchAsync(string query) =>
            await _context.Products
                .AsNoTracking().Include(p => p.Images)
                .Where(p => p.Name.Contains(query) || p.Category.Contains(query))
                .ToListAsync();

        public async Task<IReadOnlyList<Product>> GetActivePaginatedAsync(int pageNumber, int pageSize) =>
            await _context.Products
                .AsNoTracking().Include(p => p.Images).Where(p => p.IsActive == true).OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> CountAsync() =>
            await _context.Products.CountAsync();

        public async Task<bool> ExistByNameAsync(string name) =>
            await _context.Products.AnyAsync(p => p.Name == name);


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

        //filtering of the products

        public async Task<List<Product>> GetFilteredAsync(ProductFilterRequest filter)
        {
            IQueryable<Product> query = _context.Products.Where(p => p.IsActive);

            // 1️⃣ FILTER
            if (filter.Featured == true)
            {
                query = query.Where(p => p.Featured);
            }

            // 2️⃣ SORT (ALWAYS APPLY A DEFAULT)
            query = filter.SortBy switch
            {
                "price" => filter.Order == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

                "rating" => filter.Order == "desc"
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating),

                "createdAt" => filter.Order == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),

                _ => query.OrderBy(p => p.Id)
            };

            // 3️⃣ PAGINATION (LAST)
            if (filter.PageSize > 0)
            {
                query = query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(ProductFilterRequest productFilter)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.Images);

            if (productFilter.Featured == true)
                query = query.Where(p => p.Featured);

            return await query.CountAsync();
        }

        public async Task<List<Product>> GetRelatedByCategoryAsync(string category, int excludeId, int limit)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Where(p => p.Category == category && p.Id != excludeId)
                .OrderBy(r => Guid.NewGuid())
                .Take(limit)
                .ToListAsync();
        }



        public async Task UpdateAsync(Product product)
        {

            //_context.Products.Update(product);
            _context.Entry(product).State = EntityState.Modified;


            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Product>> GetAllArchievedAsync()
        {
            return await _context.Products
                .IgnoreQueryFilters().Where(p => !p.IsActive).Include(p => p.Images).ToListAsync();
        }

        public async Task<Product?> GetByIdWithDeletedAsync(int id)
        {
            return await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

       

    }
}
