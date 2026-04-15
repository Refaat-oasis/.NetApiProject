using ApiProject.Models;
using System.Linq.Expressions;

namespace ApiProject.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        // Custom methods for Products, like getting only active (not deleted) products
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>> filter = null);
        Task SoftDeleteAsync(int id);
    }
}
