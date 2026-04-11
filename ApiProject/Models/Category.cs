using System.ComponentModel.DataAnnotations;

namespace ApiProject.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        // Navigation property for products
        public ICollection<Product>? Products { get; set; }
    }
}
