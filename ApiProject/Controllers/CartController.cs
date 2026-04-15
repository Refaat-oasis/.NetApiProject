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
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;

        public CartController(ICartRepository cartRepo, IProductRepository productRepo)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var items = await _cartRepo.GetCartByUserIdAsync(userId);
            var response = items.Select(ci => new CartItemResponseDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? "",
                ProductImage = ci.Product?.ImageUrl ?? "",
                ProductPrice = ci.Product?.Price ?? 0,
                Quantity = ci.Quantity,
                Subtotal = ci.Quantity * (ci.Product?.Price ?? 0)
            });

            return Ok(response);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Check if product exists
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null || product.IsDeleted)
                return NotFound("Product not found");

            // Check if this product is already in the user's cart
            var existingItems = await _cartRepo.GetCartByUserIdAsync(userId);
            var existingItem = existingItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                // Update quantity instead of adding duplicate
                existingItem.Quantity += dto.Quantity;
                _cartRepo.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UserId = userId
                };
                await _cartRepo.AddAsync(cartItem);
            }

            await _cartRepo.SaveChangesAsync();

            // Return updated cart
            return Ok(new { message = "Item added to cart" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateCartQuantityDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var item = await _cartRepo.GetByIdAsync(id);
            if (item == null || item.UserId != userId) return NotFound();

            item.Quantity = dto.Quantity;
            _cartRepo.Update(item);
            await _cartRepo.SaveChangesAsync();

            return Ok(new { message = "Quantity updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var item = await _cartRepo.GetByIdAsync(id);
            if (item == null || item.UserId != userId) return NotFound();

            _cartRepo.Delete(item);
            await _cartRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var items = await _cartRepo.FindAsync(c => c.UserId == userId);
            foreach (var item in items)
            {
                _cartRepo.Delete(item);
            }
            await _cartRepo.SaveChangesAsync();

            return NoContent();
        }
    }
}
