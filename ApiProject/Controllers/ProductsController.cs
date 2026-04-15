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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
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
            if (productDto == null) return NotFound();
            return Ok(productDto);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string term, int? categoryId)
        {
            var products = await _productRepo.GetAllAsync(p =>
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var products = await _productRepo.GetAllAsync(p =>
                p.SellerId == userId && !p.IsDeleted
            );

            return Ok(products);
        }

        [HttpPost]


        [Authorize(Roles = "Seller")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateProduct dto)
        {
            var userId = GetUserId();
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
                IsDeleted = false ,
                SellerId = userId
            };

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [Authorize(Roles = "Seller")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProduct dto)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingProduct = await _productRepo.GetByIdAsync(id);

            if (existingProduct == null || existingProduct.IsDeleted)
                return NotFound();
            if (existingProduct.SellerId != userId)
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

        [Authorize(Roles = "Seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var product = await _productRepo.GetByIdAsync(id);

            if (product == null) return NotFound();

            if (product.SellerId != userId)
                return Forbid();

            if (!string.IsNullOrEmpty(product.Image))
            {
                var fileName = Path.GetFileName(product.Image);
                var filePath = Path.Combine(_env.WebRootPath, "images/products", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

       
            await _productRepo.SoftDeleteAsync(id);

            return NoContent();
        }
    }
}