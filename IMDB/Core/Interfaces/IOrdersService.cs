using IMDB.Data.Models;

namespace IMDB.Core.Interfaces
{
    public interface IOrdersService
    {
        Task StoreOrderAsync(List<ShoppingCartItem> items, string userId, string userEmailAddress, string paymentMethod, string paymentStatus);
        Task<List<Order>> GetOrderByUserIdAndRoleAsync(string userId, string userRole);
        Task CancelOrderAsync(int orderId);
        Task<Order> GetOrderByIdAsync(int orderId);
        //Task  DeleteOrdersByUserIdAsync(string userId);
    }
}
