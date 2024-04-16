using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class LicenseOptionalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "LicenseOptionalId,OptionalName,Id,Description,Price,Coefficient,Currency";

        public LicenseOptionalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LicenseOptionals
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.LicenseOptionals.Include(l => l.SubProduct);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: LicenseOptionals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseOptional = await _context.LicenseOptionals
                .Include(l => l.SubProduct)
                .FirstOrDefaultAsync(m => m.LicenseOptionalId == id);
            if (licenseOptional == null)
            {
                return NotFound();
            }

            return View(licenseOptional);
        }

        // GET: LicenseOptionals/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.SubProducts, "Id", "Name");
            return View();
        }

        // POST: LicenseOptionals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] LicenseOptional licenseOptional)
        {
            if (ModelState.IsValid)
            {
                _context.Add(licenseOptional);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.SubProducts, "Id", "Name", licenseOptional.SubProductId);
            return View(licenseOptional);
        }

        // GET: LicenseOptionals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseOptional = await _context.LicenseOptionals.FindAsync(id);
            if (licenseOptional == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.SubProducts, "Id", "Name", licenseOptional.SubProductId);
            return View(licenseOptional);
        }

        // POST: LicenseOptionals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] LicenseOptional licenseOptional)
        {
            if (id != licenseOptional.LicenseOptionalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(licenseOptional);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LicenseOptionalExists(licenseOptional.LicenseOptionalId))
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
            ViewData["Id"] = new SelectList(_context.SubProducts, "Id", "Name", licenseOptional.SubProductId);
            return View(licenseOptional);
        }

        // GET: LicenseOptionals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseOptional = await _context.LicenseOptionals
                .Include(l => l.SubProduct)
                .FirstOrDefaultAsync(m => m.LicenseOptionalId == id);
            if (licenseOptional == null)
            {
                return NotFound();
            }

            return View(licenseOptional);
        }

        // POST: LicenseOptionals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var licenseOptional = await _context.LicenseOptionals.FindAsync(id);
            _context.LicenseOptionals.Remove(licenseOptional);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LicenseOptionalExists(int id)
        {
            return _context.LicenseOptionals.Any(e => e.LicenseOptionalId == id);
        }
    }
}
