using ApiProject.Models;

namespace ApiProject.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        // Add specific methods for Order if needed, like GetOrdersByUserId
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
    }
}
