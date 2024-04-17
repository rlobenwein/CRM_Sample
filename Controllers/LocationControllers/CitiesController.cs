using CRM_Sample.Data;
using CRM_Sample.Models.LocationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using State = CRM_Sample.Models.LocationModels.State;

namespace CRM_Sample.Controllers.LocationControllers
{
    public class CitiesController : Controller
    {
        private const string BIND_STRING = "Id,Name,StateId";
        private readonly ApplicationDbContext _context;
        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cities
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var state = await _context.States.FirstOrDefaultAsync(s => s.Id == id);

            if (state == null)
            {
                return NotFound();
            }
            CreateViewData(state);

            var cities = _context.Cities
                .Where(c => c.StateId == state.Id).OrderBy(s => s.Name)
                .Include(s => s.State)
                .ToList();

            return View(cities);
        }

        // GET: Cities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // GET: Cities/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var state = await _context.States.FirstOrDefaultAsync(c => c.Id == id);
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == state.CountryId);
            City city = new City() { StateId = state.Id };

            if (state == null)
            {
                return NotFound();
            }
            CreateViewData(state, country);

            return PartialView(city);
        }

        // POST: Cities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind(BIND_STRING)] City city)
        {
            if (id == null || id != city.StateId)
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro");
                return RedirectToAction("Index", "Countries");
            }
            var state = await _context.States.FirstOrDefaultAsync(c => c.Id == id);
            var newCity = new City()
            {
                Name = city.Name,
                StateId = city.StateId,
            };
            if (ModelState.IsValid)
            {
                _context.Add(newCity);
                await _context.SaveChangesAsync();
                CreateViewData(state);
                return View();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro");
            }
            CreateViewData(state);
            return PartialView(city);
        }

        // GET: Cities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            var state = await _context.States.FindAsync(city.StateId);
            var country = await _context.Countries.FindAsync(state.CountryId);
            CreateViewData(state, country);
            return View(city);
        }

        // POST: Cities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] City city)
        {
            if (id != city.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(city.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var state = await _context.States.FindAsync(city.StateId);
            var country = await _context.Countries.FindAsync(state.CountryId);
            CreateViewData(state, country);
            return View(city);
        }

        // GET: Cities/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // POST: Cities/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }
        private void CreateViewData(State state, Country country = null)
        {
            ViewData["SelectedCountry"] = country?.Name;
            ViewData["SelectedState"] = state.Name;
        }
    }
}
