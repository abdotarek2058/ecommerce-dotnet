using IMDB.Core.ViewModel;
using IMDB.Data.Base;
using IMDB.Data.Models;

namespace IMDB.Core.Interfaces
{
    public interface IMoviesService : IEntityBaseRepositry<Movie>
    {
        Task<Movie> GetMovieByIdAsync(int id);
        Task<IEnumerable<Movie>> FilterAsync(string searchString);
        Task<NewMovieDropdowns> GetNewMovieDropdownsValues();
        Task AddNewMovieAsync(NewMovie data);
        Task UpdateMovieAsync(NewMovie data);
        Task<PaginatedResult<Movie>> GetAllPagedAsync(int page, int pageSize, string search);
    }
}
