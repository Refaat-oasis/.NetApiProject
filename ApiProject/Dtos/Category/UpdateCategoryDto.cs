using Microsoft.AspNetCore.Http;

namespace ApiProject.Dtos.Category
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
