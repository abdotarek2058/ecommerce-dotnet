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
    [Authorize(Roles = UserRoles.Admin)]
    public class ProducersController : Controller
    {
        private readonly IProducersService _service;
        private readonly IWebHostEnvironment _environment;
        public ProducersController(IProducersService service, IWebHostEnvironment environment)
        {
            _service = service;
            _environment = environment;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var allProcedures = await _service.GetAllAsync();
            return View(allProcedures);
        }
        //get: Procedures/Details/1
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var procedureDetails = await _service.GetByIdAsync(id);
            if (procedureDetails == null) return View("NotFound");
            return View(procedureDetails);
        }
        //get: Procedures/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,ProfilePictureURL,Bio")] Producer producer, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                ModelState.Remove(nameof(Producer.ProfilePictureURL));
                producer.ProfilePictureURL = await ImageHelper.ProcessImageAsync(imageFile, _environment.WebRootPath, "Producers");
            }

            if (!ModelState.IsValid)
            {
                return View(producer);
            }
            await _service.AddAsync(producer);
            return RedirectToAction("Index");
        }
        //get: Procedures/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var procedureDetails = await _service.GetByIdAsync(id);
            if (procedureDetails == null) return View("NotFound");
            return View(procedureDetails);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,ProfilePictureURL,Bio")] Producer producer, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                ModelState.Remove(nameof(Producer.ProfilePictureURL));
                producer.ProfilePictureURL = await ImageHelper.ProcessImageAsync(imageFile, _environment.WebRootPath, "Producers");
            }

            if (!ModelState.IsValid)
            {
                return View(producer);
            }
            await _service.UpdateAsync(id, producer);
            return RedirectToAction("Index");
        }
        //get: Procedures/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            var procedureDetails = await _service.GetByIdAsync(id);
            if (procedureDetails == null) return View("NotFound");
            return View(procedureDetails);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procedureDetails = await _service.GetByIdAsync(id);
            if (procedureDetails == null) return View("NotFound");
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
