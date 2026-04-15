using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 10000)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string ImageUrl { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        public string Image { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        //aya
        public string SellerId { get; set; }
        [ForeignKey("SellerId")]
        public ApplicationUser Seller { get; set; }
    }
}
