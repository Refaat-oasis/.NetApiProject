using ApiProject.Dtos.Product;

namespace ApiProject.Dtos.Category
{
    public class GetCategoryWithProducts
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<GetProducts> Products { get; set; } = new();
    }
}

