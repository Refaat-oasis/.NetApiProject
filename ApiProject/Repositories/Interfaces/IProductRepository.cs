using ApiProject.Models;

namespace ApiProject.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        // Custom methods for Products, like getting only active (not deleted) products
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task SoftDeleteAsync(int id);
    }
}
