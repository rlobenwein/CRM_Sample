using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models;
using RLBW_ERP.Models.LocationModels;

namespace RLBW_ERP.Controllers.LocationControllers
{
    public class StatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: States
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);

            if (country == null)
            {
                return NotFound();
            }

            ViewData["SelectedCountry"] = country.Name;
            ViewData["Id"] = country.Id;

            var states = _context.States
                .Where(s => s.CountryId == country.Id)
                .OrderBy(s => s.Name)
                .Include(s => s.Country)
                .ToList();

            return View(states);
        }

        // GET: States/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var state = await _context.States
                .Include(s => s.Country)
                .Include(s => s.Cities)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == state.CountryId);
            ViewData["SelectedCountry"] = country.Name;
            ViewData["SelectedState"] = state.Name;
            ViewData["Id"] = country.Id;
            ViewData["Id"] = state.Id;

            return View(state);
        }

        // GET: States/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);
            State state = new() { CountryId = country.Id };

            if (country == null)
            {
                return NotFound();
            }
            ViewData["SelectedCountry"] = country.Name;

            return PartialView(state);
        }

        // POST: States/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id,Name,Acronym,Id")] State state)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (id == null || id != state.CountryId)
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro");

                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Index", "Countries");
            }
            else
            {
                state.CountryId = (int)id;
            }
            if (ModelState.IsValid)
            {
                _context.Add(state);
                await _context.SaveChangesAsync();
                return View();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro");
            }
            ViewData["Id"] = new SelectList(_context.Countries, "Id", "Iso3", state.Id);

            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Countries");
        }

        // GET: States/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States.FindAsync(id);
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == state.CountryId);

            if (state == null)
            {
                return NotFound();
            }
            ViewData["SelectedCountry"] = country.Name;
            ViewData["SelectedState"] = state.Name;
            ViewData["Id"] = country.Id;
            ViewData["Id"] = state.Id;

            return View(state);
        }

        // POST: States/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Acronym,Id")] State state)
        {
            if (id != state.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(state);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StateExists(state.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Countries, "Id", "Iso3", state.Id);
            return View(state);
        }

        // GET: States/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        // POST: States/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var state = await _context.States.FindAsync(id);
            _context.States.Remove(state);
            await _context.SaveChangesAsync();
            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StateExists(int id)
        {
            return _context.States.Any(e => e.Id == id);
        }
    }
}
