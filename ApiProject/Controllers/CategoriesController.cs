using ApiProject.Dtos.Category;
using ApiProject.Dtos.Product;
using ApiProject.Models;
using ApiProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly IWebHostEnvironment _env;

        public CategoriesController(ICategoryRepository categoryRepo, IProductRepository productRepo, IWebHostEnvironment env)
        {
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepo.GetAllAsync();

            var result = categories.Select(c => new GetCategory
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var result = new GetCategory
            { 
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl
            };

            return Ok(result);
        }
        
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetCategoryProducts(int id)
        {
            var category = await _categoryRepo.GetCategoryWithProductsAsync(id);

            if (category == null) return NotFound();

            var result = new GetCategoryWithProducts
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                Products = category.Products.Select(p => new GetProducts
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Image = p.Image,
                    Stock = p.Stock, 
                    CategoryId = p.CategoryId,
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateCategory dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string imageUrl = "images/categories/default.jpg";

            if (dto.Image != null)
            {
                imageUrl = await SaveImage(dto.Image);
            }

            var category = new Category
            {
                Name = dto.Name,
                ImageUrl = imageUrl
            };

            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingCategory = await _categoryRepo.GetByIdAsync(id);
            if (existingCategory == null) return NotFound();

            existingCategory.Name = dto.Name;
            
            if (dto.Image != null)
            {
                // Delete old image if it's not the default one
                DeleteImage(existingCategory.ImageUrl);
                // Save new image
                existingCategory.ImageUrl = await SaveImage(dto.Image);
            }

            _categoryRepo.Update(existingCategory);
            await _categoryRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            // 1. Get or create the "Others" category
            var othersCategory = await GetOrCreateOthersCategory();

            // 2. If trying to delete the "Others" category itself, refuse if it has products
            if (id == othersCategory.Id)
            {
                var productsInOthers = await _productRepo.FindAsync(p => p.CategoryId == id && !p.IsDeleted);
                if (productsInOthers.Any())
                {
                    return BadRequest(new { Message = "Cannot delete the 'Others' category while it contains products." });
                }
            }
            else
            {
                // 3. Move all products in the current category to "Others"
                var productsInCategory = await _productRepo.FindAsync(p => p.CategoryId == id && !p.IsDeleted);
                foreach (var product in productsInCategory)
                {
                    product.CategoryId = othersCategory.Id;
                    _productRepo.Update(product);
                }
                await _productRepo.SaveChangesAsync();
            }

            // 4. Delete the category
            DeleteImage(category.ImageUrl);
            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}/with-products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWithProducts(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            // 1. Get or create the "Others" category (since we'll move products there)
            var othersCategory = await GetOrCreateOthersCategory();

            // 2. Locate all relevant products
            var productsInCategory = await _productRepo.FindAsync(p => p.CategoryId == id);
            
            foreach (var product in productsInCategory)
            {
                // 3. Mark as inactive AND move to Others for future reactivation safety
                product.IsDeleted = true;
                if (id != othersCategory.Id)
                {
                    product.CategoryId = othersCategory.Id;
                }
                _productRepo.Update(product);
            }
            await _productRepo.SaveChangesAsync();

            // 4. Delete the category
            DeleteImage(category.ImageUrl);
            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync();

            return NoContent();
        }

        private async Task<Category> GetOrCreateOthersCategory()
        {
            var categories = await _categoryRepo.GetAllAsync();
            var others = categories.FirstOrDefault(c => c.Name.Equals("Others", StringComparison.OrdinalIgnoreCase));

            if (others == null)
            {
                others = new Category
                {
                    Name = "Others",
                    ImageUrl = "images/categories/default.jpg"
                };
                await _categoryRepo.AddAsync(others);
                await _categoryRepo.SaveChangesAsync();
            }

            return others;
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images/categories");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "images/categories/" + fileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.Contains("default.jpg"))
            {
                var filePath = Path.Combine(_env.WebRootPath, imageUrl.Replace("/", "\\"));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
