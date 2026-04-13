using IMDB.Core.Static;
using IMDB.Core.ViewModel;
using IMDB.Data;
using IMDB.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Dashboard()
        {
            var admin = await _userManager.GetUserAsync(User);

            var movies = await _context.Movies
                .Include(m => m.Cinema)
                .AsNoTracking()
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Movie)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalMovies = movies.Count;
            var totalUsers = await _context.Users.CountAsync();
            var totalOrders = orders.Count;
            var pendingOrders = orders.Count(o => o.OrderStatus == Data.Enums.OrderStatus.Pending);
            var totalRevenue = orders
                .Where(o => o.OrderStatus != Data.Enums.OrderStatus.Cancelled)
                .SelectMany(o => o.OrderItems)
                .Sum(oi => oi.Price * oi.Amount);

            var heroMovie = movies
                .OrderByDescending(m => m.Rating)
                .ThenByDescending(m => m.StartDate)
                .FirstOrDefault();

            var viewModel = new DashboardViewModel
            {
                AdminName = admin?.FullName ?? "Admin",
                AdminPicture = admin?.ProfilePicture ?? User.FindFirst("picture")?.Value,
                HeroMovie = heroMovie == null ? null : new DashboardHeroMovieViewModel
                {
                    Id = heroMovie.Id,
                    Name = heroMovie.Name,
                    Description = heroMovie.Description.Length > 180 ? heroMovie.Description[..180] + "..." : heroMovie.Description,
                    ImageUrl = heroMovie.ImageURL,
                    Rating = heroMovie.Rating,
                    Category = heroMovie.MovieCategory.ToString(),
                    Cinema = heroMovie.Cinema?.Name ?? "Unknown cinema"
                },
                Stats = new List<DashboardStatCardViewModel>
                {
                    new() { Title = "Movies", Value = totalMovies.ToString(), Subtitle = "Titles in catalog", Icon = "bi-film", ToneClass = "tone-gold" },
                    new() { Title = "Users", Value = totalUsers.ToString(), Subtitle = "Registered accounts", Icon = "bi-people", ToneClass = "tone-blue" },
                    new() { Title = "Orders", Value = totalOrders.ToString(), Subtitle = $"{pendingOrders} pending now", Icon = "bi-bag-check", ToneClass = "tone-green" },
                    new() { Title = "Revenue", Value = totalRevenue.ToString("0.00"), Subtitle = "Completed and pending sales", Icon = "bi-cash-stack", ToneClass = "tone-rose" }
                },
                QuickLinks = new List<DashboardQuickLinkViewModel>
                {
                    new() { Title = "Movies", Subtitle = "Manage films and posters", Icon = "bi-camera-reels", Controller = "Movies", Action = "Index" },
                    new() { Title = "Users", Subtitle = "Review accounts and roles", Icon = "bi-person-badge", Controller = "Account", Action = "Users" },
                    new() { Title = "Orders", Subtitle = "Track purchases and statuses", Icon = "bi-receipt", Controller = "Orders", Action = "Index" },
                    new() { Title = "Actors", Subtitle = "Update cast profiles", Icon = "bi-stars", Controller = "Actors", Action = "Index" },
                    new() { Title = "Producers", Subtitle = "Edit producers and bios", Icon = "bi-person-workspace", Controller = "Producers", Action = "Index" },
                    new() { Title = "Cinemas", Subtitle = "Maintain cinema partners", Icon = "bi-building", Controller = "Cinemas", Action = "Index" }
                },
                TrendingMovies = movies
                    .OrderByDescending(m => m.Rating)
                    .ThenByDescending(m => m.StartDate)
                    .Take(5)
                    .Select(m => new DashboardMovieCardViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Category = m.MovieCategory.ToString(),
                        ImageUrl = m.ImageURL,
                        Rating = m.Rating,
                        Status = m.EndDate < DateTime.Now ? "Expired" : m.StartDate > DateTime.Now ? "Upcoming" : "Available",
                        StatusClass = m.EndDate < DateTime.Now ? "status-expired" : m.StartDate > DateTime.Now ? "status-upcoming" : "status-available"
                    })
                    .ToList(),
                RecentOrders = orders
                    .Take(6)
                    .Select(o => new DashboardOrderRowViewModel
                    {
                        Id = o.Id,
                        CustomerName = o.User?.FullName ?? o.Email,
                        MovieName = o.OrderItems.FirstOrDefault()?.Movie?.Name ?? "No movie",
                        Total = o.OrderItems.Sum(i => i.Price * i.Amount).ToString("0.00"),
                        Date = o.OrderDate.ToString("dd MMM yyyy - hh:mm tt"),
                        StatusText = o.OrderStatus.ToString(),
                        StatusClass = o.OrderStatus switch
                        {
                            Data.Enums.OrderStatus.Completed => "status-available",
                            Data.Enums.OrderStatus.Pending => "status-pending",
                            _ => "status-expired"
                        }
                    })
                    .ToList(),
                CategoryStats = movies
                    .GroupBy(m => m.MovieCategory)
                    .Select((group, index) => new DashboardCategoryStatViewModel
                    {
                        Category = group.Key,
                        Count = group.Count(),
                        Percentage = totalMovies == 0 ? 0 : Math.Round(group.Count() * 100.0 / totalMovies, 1),
                        GradientClass = (index % 4) switch
                        {
                            0 => "gradient-a",
                            1 => "gradient-b",
                            2 => "gradient-c",
                            _ => "gradient-d"
                        }
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList()
            };

            return View(viewModel);
        }

    }
}
