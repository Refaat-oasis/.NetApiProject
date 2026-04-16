using ApiProject.DTOs;
using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;

        private static readonly string[] AllowedPaymentMethods = 
            { "CreditCard", "PayPal", "CashOnDelivery", "Wallet" };

        public OrdersController(IOrderRepository orderRepo, ICartRepository cartRepo, IProductRepository productRepo)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var orders = await _orderRepo.GetOrdersByUserIdAsync(userId);
            var response = orders.Select(o => MapToOrderResponse(o));
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null || order.UserId != userId) return NotFound();

            return Ok(MapToOrderResponse(order));
        }

        [HttpPost("checkout")]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate payment method
            if (!AllowedPaymentMethods.Contains(dto.PaymentMethod))
                return BadRequest(new { message = $"Invalid payment method. Allowed: {string.Join(", ", AllowedPaymentMethods)}" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isGuest = string.IsNullOrEmpty(userId);

            // Guest checkout validation
            if (isGuest)
            {
                if (string.IsNullOrWhiteSpace(dto.GuestEmail))
                    return BadRequest(new { message = "Email is required for guest checkout" });
                if (string.IsNullOrWhiteSpace(dto.GuestName))
                    return BadRequest(new { message = "Name is required for guest checkout" });
            }

            // Get cart items
            IEnumerable<CartItem> cartItems;
            if (isGuest)
            {
                // For guest checkout, we expect items to be synced to a temporary guest cart
                // Guest users won't have server-side cart, so we return error
                return BadRequest(new { message = "Please login to complete checkout, or register an account" });
            }

            cartItems = await _cartRepo.GetCartByUserIdAsync(userId!);
            if (!cartItems.Any())
                return BadRequest(new { message = "Cart is empty" });

            // Stock validation
            var stockErrors = new List<string>();
            foreach (var ci in cartItems)
            {
                var product = await _productRepo.GetByIdAsync(ci.ProductId);
                if (product == null || product.IsDeleted)
                {
                    stockErrors.Add($"Product '{ci.Product?.Name ?? ci.ProductId.ToString()}' is no longer available");
                    continue;
                }
                if (product.Stock < ci.Quantity)
                {
                    stockErrors.Add($"'{product.Name}' only has {product.Stock} in stock (requested {ci.Quantity})");
                }
            }

            if (stockErrors.Any())
                return BadRequest(new { message = "Stock validation failed", errors = stockErrors });

            // Create the Order
            var order = new Order
            {
                UserId = userId!,
                OrderDate = DateTime.Now,
                Status = "Processing",
                PaymentMethod = dto.PaymentMethod,
                ShippingAddress = dto.ShippingAddress,
                GuestEmail = isGuest ? dto.GuestEmail : null,
                GuestName = isGuest ? dto.GuestName : null,
                Total = cartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0)),
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product?.Price ?? 0
                }).ToList()
            };

            // Decrement stock
            foreach (var ci in cartItems)
            {
                var product = await _productRepo.GetByIdAsync(ci.ProductId);
                if (product != null)
                {
                    product.Stock -= ci.Quantity;
                    _productRepo.Update(product);
                }
            }

            // Save Order
            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            // Clear Cart
            foreach (var item in cartItems)
            {
                _cartRepo.Delete(item);
            }
            await _cartRepo.SaveChangesAsync();

            // Decrement stock save
            await _productRepo.SaveChangesAsync();

            return Ok(new OrderResponseDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Total = order.Total,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                ShippingAddress = order.ShippingAddress,
                Items = order.OrderItems?.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Subtotal = oi.Quantity * oi.Price
                }).ToList() ?? new()
            });
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            var response = orders.Select(o => MapToOrderResponse(o));
            return Ok(response);
        }

        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderAdmin(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return Ok(MapToOrderResponse(order));
        }

        [HttpPut("admin/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.Status = dto.Status;
            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully", status = order.Status });
        }


        private static OrderResponseDto MapToOrderResponse(Order o)
        {
            return new OrderResponseDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Total = o.Total,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                ShippingAddress = o.ShippingAddress,
                Items = o.OrderItems?.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Subtotal = oi.Quantity * oi.Price
                }).ToList() ?? new()
            };
        }
    }
}
