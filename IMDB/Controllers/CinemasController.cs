using IMDB.Core.Interfaces;
using IMDB.Core.Static;
using IMDB.Data;
using IMDB.Data.Models;
using IMDB.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMDB.Controllers
{
    [Authorize(Roles =UserRoles.Admin)]
    public class CinemasController : Controller
    {
        private readonly ICinemasService _service;
        private readonly IWebHostEnvironment _environment;
        public CinemasController(ICinemasService service, IWebHostEnvironment environment)
        {
            _service = service;
            _environment = environment;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var allCinemas = await _service.GetAllAsync();
            return View(allCinemas);
        }
        //get: Cinemas/Details/1
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            return View(cinemaDetails);
        }
        //get: Cinemas/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Logo,Name,Description")] Cinema cinema, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                ModelState.Remove(nameof(Cinema.Logo));
                cinema.Logo = await ImageHelper.ProcessImageAsync(imageFile, _environment.WebRootPath, "Cinemas");
            }

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }
            await _service.AddAsync(cinema);
            return RedirectToAction("Index");
        }
        //get: Cinemas/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            return View(cinemaDetails);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Logo,Name,Description")] Cinema cinema, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                ModelState.Remove(nameof(Cinema.Logo));
                cinema.Logo = await ImageHelper.ProcessImageAsync(imageFile, _environment.WebRootPath, "Cinemas");
            }

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }
            await _service.UpdateAsync(id, cinema);
            return RedirectToAction("Index");
        }
        //get: Cinemas/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            return View(cinemaDetails);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
