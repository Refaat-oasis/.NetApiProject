using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires the user to be logged in (JWT Token)
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;

        public OrdersController(IOrderRepository orderRepo, ICartRepository cartRepo)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var orders = await _orderRepo.FindAsync(o => o.UserId == userId);
            return Ok(orders);
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // 1. Get items from user's cart
            var cartItems = await _cartRepo.FindAsync(c => c.UserId == userId);
            if (!cartItems.Any()) return BadRequest("Cart is empty");

            // 2. Create the Order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Processing",
                Total = cartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0)),
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product?.Price ?? 0 // Snapshot of the price
                }).ToList()
            };

            // 3. Save Order and Clear Cart
            await _orderRepo.AddAsync(order);
            foreach (var item in cartItems)
            {
                _cartRepo.Delete(item);
            }

            await _orderRepo.SaveChangesAsync();
            await _cartRepo.SaveChangesAsync();

            return Ok(order);
        }
    }
}
