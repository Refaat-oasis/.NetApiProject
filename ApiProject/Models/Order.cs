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

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(200)]
        public string? GuestEmail { get; set; }

        [StringLength(100)]
        public string? GuestName { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
