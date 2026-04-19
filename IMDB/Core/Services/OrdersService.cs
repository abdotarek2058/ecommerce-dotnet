using Hangfire;
using IMDB.Core.Interfaces;
using IMDB.Data;
using IMDB.Data.Enums;
using IMDB.Data.Models;
using Microsoft.EntityFrameworkCore;
namespace IMDB.Core.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrdersService> _logger;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        public OrdersService(AppDbContext context, ILogger<OrdersService> logger, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
          
        }
        public async Task<List<Order>> GetOrderByUserIdAndRoleAsync(string userId, string userRole)
        {
            _logger.LogInformation("Fetching orders for user {UserId} with role {Role}", userId, userRole);
            var orders = _context.Orders.Include(n => n.OrderItems).ThenInclude(n => n.Movie).Include(n => n.User).AsQueryable();

            if (userRole != "Admin")
            {
                orders = orders.Where(n => n.UserId == userId);
            }
            var result = await orders.ToListAsync();
            _logger.LogInformation("Retrieved {Count} orders for user {UserId}", result.Count, userId);
            return result;
        }

        public async Task StoreOrderAsync(List<ShoppingCartItem> items, string userId, string userEmailAddress, string paymentMethod, string paymentStatus)
        {
            try
            {
                _logger.LogInformation("Starting StoreOrderAsync for user {UserId}", userId);
                var order = new Order()
                {
                    UserId = userId,
                    Email = userEmailAddress,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = paymentStatus,
                    OrderStatus = paymentMethod == "PayPal" ? OrderStatus.Completed : OrderStatus.Pending,
                    OrderDate = DateTime.Now
                };

                await _context.Orders.AddAsync(order);

                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    var orderItem = new OrderItem()
                    {
                        Amount = item.Amount,
                        MovieId = item.Movie.Id,
                        OrderId = order.Id,
                        Price = item.Movie.Price
                    };

                    await _context.OrderItems.AddAsync(orderItem);
                }
                _logger.LogInformation("Creating order with payment method {PaymentMethod}", paymentMethod);

                if (items == null || !items.Any())
                {
                    _logger.LogWarning("Attempted to create order with empty cart for user {UserId}", userId);
                    throw new Exception("Shopping cart is empty");
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order created successfully with ID {OrderId}", order.Id);
                try
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "Data/EmailTemplates/OrderConfirmation.html");
                    var template = await File.ReadAllTextAsync(path);

                    var baseUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" +
                                  _httpContextAccessor.HttpContext.Request.Host;
                    template = template.Replace("{{LINK}}", $"{baseUrl}/Orders");
                    template = template.Replace("{{NAME}}", userEmailAddress);
                    template = template.Replace("{{ORDERID}}", order.Id.ToString());
                    

                    template = template.Replace("{{DATE}}", order.OrderDate.ToString("dd MMM yyyy - hh:mm tt"));


                    string itemsHtml = "";
                    decimal total = 0;

                    foreach (var item in items)
                    {
                        var movie = item.Movie;

                        var row = $@"
                            <tr>
                                <td style='padding:10px'>{movie.Name}</td>
                                <td style='padding:10px; text-align:center'>{item.Amount}</td>
                                <td style='padding:10px; text-align:right'>{movie.Price} EGP</td>
                            </tr>";

                        itemsHtml += row;

                        total += (decimal)(movie.Price * item.Amount);
                    }

                    template = template.Replace("{{ITEMS}}", itemsHtml);
                    template = template.Replace("{{TOTAL}}", total.ToString());

                    BackgroundJob.Enqueue<IEmailService> (x => x.SendEmailAsync(
                       userEmailAddress,
                       $"Order #{order.Id} Confirmation",
                       template
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email for order {OrderId}", order.Id);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while storing order for user {UserId}", userId);
                throw; // Rethrow the exception after logging it
            }
        }
        public async Task CancelOrderAsync(int orderId)
        {
            _logger.LogInformation("Attempting to cancel order {OrderId}", orderId);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return;
            }

            if (order != null)
            {
                order.OrderStatus = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
            }
        }
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            _logger.LogInformation("Fetching order with ID {OrderId}", orderId);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", orderId);
            }
            else
            {
                _logger.LogInformation("Order with ID {OrderId} retrieved successfully", orderId);
            }

            return order;
        }

        //public async Task DeleteOrdersByUserIdAsync(string  userId)
        //{
        //    var orders = await _context.Orders.Where(o=>o.UserId == userId).ToListAsync();
        //    foreach (var order in orders)
        //    {
        //        var orderItems = await _context.OrderItems
        //            .Where(oi => oi.OrderId == order.Id)
        //            .ToListAsync();

        //        _context.OrderItems.RemoveRange(orderItems);
        //    }
        //    _context.Orders.RemoveRange(orders);
        //    await _context.SaveChangesAsync();
        //}

    }
}
