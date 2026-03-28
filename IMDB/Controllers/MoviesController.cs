using IMDB.Core.Interfaces;
using IMDB.Core.Static;
using IMDB.Core.ViewModel;
using IMDB.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMDB.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class MoviesController : Controller
    {
        private async Task PopulateDropdowns()
        {
                var movieDropdownsData = await _service.GetNewMovieDropdownsValues();
                ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");
                ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
        }
        private readonly IMoviesService _service;
        public MoviesController(IMoviesService service)
        {
            _service = service;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            int pageSize = 6;
            var result = await _service.GetAllPagedAsync(page, pageSize, search);
            ViewBag.Search = search;

            return View(result);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Filter(string searchString)
        {
            var allMovies = await _service.FilterAsync(searchString);
            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = allMovies.Where(n => n.Name.ToLower().Contains(searchString.ToLower()) || n.Description.ToLower().Contains(searchString.ToLower()) || n.MovieCategory.ToString().ToLower().Contains(searchString.ToLower())).ToList();
                return View("Index", filteredResult);
            }
            return View("Index", allMovies);
        }
        //Get: Movies/Details/1
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var movieDetails = await _service.GetMovieByIdAsync(id);
            if (movieDetails == null)
                return View("NotFound");

            return View(movieDetails);
        }
        //Get: Movies/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewMovie movie)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(movie);
            }
            await _service.AddNewMovieAsync(movie);
            return RedirectToAction(nameof(Index));
        }
        //Get: Movies/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var movieDetails = await _service.GetMovieByIdAsync(id);
            if (movieDetails == null) return View("NotFound");
            var updatemovie = new NewMovie()
            {
                Id = movieDetails.Id,
                Name = movieDetails.Name,
                Description = movieDetails.Description,
                Price = movieDetails.Price,
                Rating = movieDetails.Rating,
                ImageURL = movieDetails.ImageURL,
                StartDate = movieDetails.StartDate,
                EndDate = movieDetails.EndDate,
                MovieCategory = movieDetails.MovieCategory,
                CinemaId = movieDetails.CinemaId,
                ProducerId = movieDetails.ProducerId,
                ActorIds = movieDetails.Actors_Movies!.Select(n => n.ActorId).ToList()
            };
            await PopulateDropdowns();
            return View(updatemovie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NewMovie movie)
        {
            if (id != movie.Id) return  BadRequest(); ;
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(movie);
            }
            await _service.UpdateMovieAsync(movie);
            return RedirectToAction(nameof(Index));
        }
    }
}
