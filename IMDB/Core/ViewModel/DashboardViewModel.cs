using IMDB.Data.Enums;

namespace IMDB.Core.ViewModel
{
    public class DashboardViewModel
    {
        public string AdminName { get; set; } = string.Empty;
        public string? AdminPicture { get; set; }
        public DashboardHeroMovieViewModel? HeroMovie { get; set; }
        public List<DashboardStatCardViewModel> Stats { get; set; } = new();
        public List<DashboardQuickLinkViewModel> QuickLinks { get; set; } = new();
        public List<DashboardMovieCardViewModel> TrendingMovies { get; set; } = new();
        public List<DashboardOrderRowViewModel> RecentOrders { get; set; } = new();
        public List<DashboardCategoryStatViewModel> CategoryStats { get; set; } = new();
    }

    public class DashboardHeroMovieViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Cinema { get; set; } = string.Empty;
    }

    public class DashboardStatCardViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ToneClass { get; set; } = string.Empty;
    }

    public class DashboardQuickLinkViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }

    public class DashboardMovieCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }

    public class DashboardOrderRowViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }

    public class DashboardCategoryStatViewModel
    {
        public MovieCategory Category { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string GradientClass { get; set; } = string.Empty;
    }
}
