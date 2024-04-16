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
    public class LicenseNetworkTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LicenseNetworkTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LicenseNetworkTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.LicenseNetworkTypes.ToListAsync());
        }

        // GET: LicenseNetworkTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseNetworkType = await _context.LicenseNetworkTypes
                .FirstOrDefaultAsync(m => m.LicenseNetworkTypeId == id);
            if (licenseNetworkType == null)
            {
                return NotFound();
            }

            return View(licenseNetworkType);
        }

        // GET: LicenseNetworkTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LicenseNetworkTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LicenseNetworkTypeId,NetworkType")] LicenseNetworkType licenseNetworkType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(licenseNetworkType);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(licenseNetworkType);
        }

        // GET: LicenseNetworkTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseNetworkType = await _context.LicenseNetworkTypes.FindAsync(id);
            if (licenseNetworkType == null)
            {
                return NotFound();
            }
            return View(licenseNetworkType);
        }

        // POST: LicenseNetworkTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LicenseNetworkTypeId,NetworkType")] LicenseNetworkType licenseNetworkType)
        {
            if (id != licenseNetworkType.LicenseNetworkTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(licenseNetworkType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LicenseNetworkTypeExists(licenseNetworkType.LicenseNetworkTypeId))
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
            return View(licenseNetworkType);
        }

        // GET: LicenseNetworkTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseNetworkType = await _context.LicenseNetworkTypes
                .FirstOrDefaultAsync(m => m.LicenseNetworkTypeId == id);
            if (licenseNetworkType == null)
            {
                return NotFound();
            }

            return View(licenseNetworkType);
        }

        // POST: LicenseNetworkTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var licenseNetworkType = await _context.LicenseNetworkTypes.FindAsync(id);
            _context.LicenseNetworkTypes.Remove(licenseNetworkType);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LicenseNetworkTypeExists(int id)
        {
            return _context.LicenseNetworkTypes.Any(e => e.LicenseNetworkTypeId == id);
        }
    }
}
