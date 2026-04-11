using ApiProject.Models;

namespace ApiProject.Repositories.Interfaces
{
    public interface ICartRepository : IRepository<CartItem>
    {
        // Add specific methods for CartItems if needed
        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId);
    }
}
