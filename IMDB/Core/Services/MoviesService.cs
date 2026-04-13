using IMDB.Core.Interfaces;
using IMDB.Core.ViewModel;
using IMDB.Data;
using IMDB.Data.Base;
using IMDB.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IMDB.Core.Services
{
    public class MoviesService :EntityBaseRepositry<Movie>, IMoviesService
    {
       
        private readonly AppDbContext _context;
        public MoviesService(AppDbContext context) : base(context) 
        {
            _context = context;
            
        }
        public async Task<Movie> GetMovieByIdAsync(int id) 
        {
            var movie = await GetByIdAsync(id, qury=>
            qury.Include(c => c.Cinema)
            .Include(p => p.Producer)
            .Include(am => am.Actors_Movies).ThenInclude(a => a.Actor)
            );
            
            return movie;
            
        }
        public async Task AddNewMovieAsync(NewMovie data)
        {
            var newMovie = new Movie()
            {
                Name = data.Name,
                Description = data.Description,
                Price = data.Price,
                ImageURL = data.ImageURL,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                MovieCategory = data.MovieCategory,
                CinemaId = data.CinemaId,
                ProducerId = data.ProducerId,
                Rating = data.Rating
            };
            await _context.Movies.AddAsync(newMovie);
            

            //Add Movie Actors
            foreach (var actorId in data.ActorIds)
            {
                var newActorMovie = new Actor_Movie()
                {
                    Movie = newMovie,
                    ActorId = actorId
                };
                await _context.Actors_Movies.AddAsync(newActorMovie);
            }
            await _context.SaveChangesAsync();
        }
        public async Task<NewMovieDropdowns> GetNewMovieDropdownsValues()
        {
            var response = new NewMovieDropdowns()
            {
                Actors = await _context.Actors.OrderBy(n => n.FullName).ToListAsync(),
                Cinemas = await _context.Cinemas.OrderBy(n => n.Name).ToListAsync(),
                Producers = await _context.Producers.OrderBy(n => n.FullName).ToListAsync()
            };
            return response;
        }
        public async Task<IEnumerable<Movie>> FilterAsync(string searchString)
        {
            var query = _context.Movies.Include(m => m.Cinema).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n =>
                    n.Name.Contains(searchString) ||
                    n.Description.Contains(searchString) ||
                    n.MovieCategory.ToString().Contains(searchString));
            }

            return await query.ToListAsync();
        }
        public async Task UpdateMovieAsync(NewMovie data)
        {
            var dbMovie = await _context.Movies.FirstOrDefaultAsync(n => n.Id == data.Id);
            if (dbMovie == null)
                throw new KeyNotFoundException("Movie not found");


            dbMovie.Name = data.Name;
            dbMovie.Description = data.Description;
            dbMovie.Price = data.Price;
            dbMovie.ImageURL = data.ImageURL;
            dbMovie.StartDate = data.StartDate;
            dbMovie.EndDate = data.EndDate;
            dbMovie.MovieCategory = data.MovieCategory;
            dbMovie.CinemaId = data.CinemaId;
            dbMovie.ProducerId = data.ProducerId;
            dbMovie.Rating = data.Rating;
            await _context.SaveChangesAsync();



            //Remove existing actors
            var existingActorsDb = _context.Actors_Movies.Where(n => n.MovieId == data.Id).ToList();
            _context.Actors_Movies.RemoveRange(existingActorsDb);
            await _context.SaveChangesAsync();
            //Add Movie Actors
            foreach (var actorId in data.ActorIds)
            {
                var newActorMovie = new Actor_Movie()
                {
                    MovieId = data.Id,
                    ActorId = actorId
                };
                await _context.Actors_Movies.AddAsync(newActorMovie);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<Movie>> GetAllPagedAsync(int page, int pageSize, string search)
        {
            var query = _context.Movies
                .Include(m => m.Cinema)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m =>
                    m.Name.Contains(search) ||
                    m.Description.Contains(search) ||
                    m.MovieCategory.ToString().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Movie>
            {
                Data = movies,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

    }
}
