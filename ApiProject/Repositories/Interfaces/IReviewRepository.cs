using ApiProject.Models;

namespace ApiProject.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        // Add specific methods for Reviews if needed
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
    }
}
