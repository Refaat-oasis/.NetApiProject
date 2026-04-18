using ApiProject.Dtos.Product;
using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly IWebHostEnvironment _env;
        public ProductsController(IProductRepository productRepo, IWebHostEnvironment env)
        {
            _productRepo = productRepo;
            _env = env;

        }
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var products = await _productRepo.GetActiveProductsAsync();

            var productDtos = products.Select(p => new GetProducts
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                Image = p.Image
            }).ToList();
            return Ok(productDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var products = await _productRepo.GetAllAsync(); // Fetches tracking IsDeleted from GenericRepository

            var productDtos = products.Select(p => new GetProducts
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                Image = p.Image,
                // Soft deletion state can be tracked or accessed if mapped in DTO, but we just return matching format for now
            }).ToList();
            return Ok(productDtos);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string term, int? categoryId)
        {
            var products = await _productRepo.FindAsync(p =>
                !p.IsDeleted &&
                (string.IsNullOrEmpty(term) ||
                 p.Name.Contains(term) ||
                 p.Description.Contains(term)) &&
                (!categoryId.HasValue || p.CategoryId == categoryId)
            );

            var result = products.Select(p => new GetProducts
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                Image = p.Image
            });

            return Ok(result);
        }

        [Authorize(Roles = "Seller")]
        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            var userId = GetUserId();

            var products = await _productRepo.FindAsync(p =>
                p.SellerId == userId && !p.IsDeleted
            );

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();

            var productDto = new GetProducts
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                Image = product.Image
            };
            return Ok(productDto);
        }

        [Authorize(Roles = "Admin,Seller")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateProduct dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string imagePath = "";

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                imagePath = "/images/products/" + fileName;
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                Image = imagePath,
                IsDeleted = false,
                SellerId = userId
            };

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [Authorize(Roles = "Admin,Seller")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProduct dto)
        {
            var userId = GetUserId();
            bool isAdmin = User.IsInRole("Admin");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingProduct = await _productRepo.GetByIdAsync(id);

            if (existingProduct == null || existingProduct.IsDeleted)
                return NotFound();

            if (!isAdmin && existingProduct.SellerId != userId)
                return Forbid();

            existingProduct.Name = dto.Name;
            existingProduct.Description = dto.Description;
            existingProduct.Price = dto.Price;
            existingProduct.Stock = dto.Stock;
            existingProduct.CategoryId = dto.CategoryId;

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                existingProduct.Image = "/images/products/" + fileName;
            }

            _productRepo.Update(existingProduct);
            await _productRepo.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            bool isAdmin = User.IsInRole("Admin");

            var product = await _productRepo.GetByIdAsync(id);

            if (product == null) return NotFound();

            if (!isAdmin && product.SellerId != userId)
                return Forbid();

            await _productRepo.SoftDeleteAsync(id);

            return NoContent();
        }

        [Authorize(Roles = "Admin,Seller")]
        [HttpPut("{id}/reactivate")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var userId = GetUserId();
            bool isAdmin = User.IsInRole("Admin");

            var product = await _productRepo.GetByIdAsync(id);

            if (product == null) return NotFound();

            if (!isAdmin && product.SellerId != userId)
                return Forbid();

            product.IsDeleted = false;
            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();

            return Ok(new { Message = "Product reactivated successfully.", Product = product });
        }
    }
}