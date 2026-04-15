using ApiProject.Data;
using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiProject.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            // Only return products where IsDeleted is false
            return await _dbSet
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>> filter = null)
        {
            IQueryable<Product> query = _context.Set<Product>();

            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync();
        }

      
        public async Task SoftDeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                Update(product);
                await SaveChangesAsync();
            }
        }
    }
}
