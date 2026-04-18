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

        public CategoriesController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepo.GetAllAsync();

            var result = categories.Select(c => new GetCategory
            {
                Id = c.Id,
                Name = c.Name
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var result = new CreateCategory
            { 
                Name = category.Name
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
                Products = category.Products.Select(p => new GetProducts
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Image = p.Image,
                    Stock = p.Stock , 
                    CategoryId = p.CategoryId,
                }).ToList()
            };

            return Ok(result);
        }
        [HttpPost]
       [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategory dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = new Category
            {
                Name = dto.Name,
                ImageUrl = "default.jpg" 
            };

            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveChangesAsync();

            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Category dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingCategory = await _categoryRepo.GetByIdAsync(id);
            if (existingCategory == null) return NotFound();

            existingCategory.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                existingCategory.ImageUrl = dto.ImageUrl;
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

            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync();

            return NoContent();
        }
    }
}
