using ApiProject.Models;

namespace ApiProject.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category> GetCategoryWithProductsAsync(int id);
    }
}
