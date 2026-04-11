using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
