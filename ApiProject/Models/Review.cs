using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int StarRating { get; set; }

        [Required]
        public int ProductId { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
