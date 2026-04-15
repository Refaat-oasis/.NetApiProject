using ApiProject.Dtos.reviews;
using ApiProject.Models;

namespace ApiProject.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        // Add specific methods for Reviews if needed

        Task<Review?> GetUserReview(int productId, string userId);
        Task<IEnumerable<ReviewDto>> GetProductReviews(int productId);
        Task AddReview(Review review);
        Task UpdateReview(Review review);
        Task<double> GetAverageRating(int productId);
    }
}
