using ApiProject.Data;
using ApiProject.Dtos.reviews;
using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Repositories.Implementations
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Review?> GetUserReview(int productId, string userId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviews(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    UserName = r.User.FullName!,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }


        public async Task AddReview(Review review)
        {
            await _context.Reviews.AddAsync(review);
            _context.SaveChanges();

        }

        public async Task UpdateReview(Review review)
        {
            _context.Reviews.Update(review);
            _context.SaveChanges();

        }

        public async Task<double> GetAverageRating(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double?)r.StarRating) ?? 0;
        }

     
    }
}