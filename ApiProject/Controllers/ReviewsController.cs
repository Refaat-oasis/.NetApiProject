using ApiProject.Dtos.reviews;
using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepo;

        public ReviewsController(IReviewRepository reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        [Authorize]


        [HttpPost]
        public async Task<IActionResult> AddOrUpdateReview(CreateReviewDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var existingReview = await _reviewRepo.GetUserReview(dto.ProductId, userId);

            if (existingReview != null)
            {
                existingReview.StarRating = dto.StarRating;
                existingReview.Comment = dto.Comment;

                await _reviewRepo.UpdateReview(existingReview);
            }
            else
            {
                var review = new Review
                {
                    ProductId = dto.ProductId,
                    StarRating = dto.StarRating,
                    Comment = dto.Comment,
                    UserId = userId
                };

                await _reviewRepo.AddReview(review);
            }

            return Ok();
        }

        // ➤ Get reviews for product
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _reviewRepo.GetProductReviews(productId);
            return Ok(reviews);
        }

        // ➤ Get average rating
        [HttpGet("average/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverageRating(int productId)
        {
            var avg = await _reviewRepo.GetAverageRating(productId);
            return Ok(avg);
        }
    }
}