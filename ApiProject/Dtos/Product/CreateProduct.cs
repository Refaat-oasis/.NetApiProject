using System.ComponentModel.DataAnnotations;

namespace ApiProject.Dtos.Product
{
    public class CreateProduct
    {
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

        public IFormFile? Image { get; set; }
        [Required]
        public int CategoryId { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
