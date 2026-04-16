using System.ComponentModel.DataAnnotations;

namespace ApiProject.Dtos.reviews
{
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 5)]
        public int StarRating { get; set; }

        public string? Comment { get; set; }
    }
}
