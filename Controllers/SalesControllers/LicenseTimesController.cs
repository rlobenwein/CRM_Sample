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
    public class LicenseTimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LicenseTimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LicenseTimes
        public async Task<IActionResult> Index()
        {
            return View(await _context.LicenseTimes.ToListAsync());
        }

        // GET: LicenseTimes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseTime = await _context.LicenseTimes
                .FirstOrDefaultAsync(m => m.LicenseTimeId == id);
            if (licenseTime == null)
            {
                return NotFound();
            }

            return View(licenseTime);
        }

        // GET: LicenseTimes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LicenseTimes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LicenseTimeId,Time")] LicenseTime licenseTime)
        {
            if (ModelState.IsValid)
            {
                _context.Add(licenseTime);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(licenseTime);
        }

        // GET: LicenseTimes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseTime = await _context.LicenseTimes.FindAsync(id);
            if (licenseTime == null)
            {
                return NotFound();
            }
            return View(licenseTime);
        }

        // POST: LicenseTimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LicenseTimeId,Time")] LicenseTime licenseTime)
        {
            if (id != licenseTime.LicenseTimeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(licenseTime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LicenseTimeExists(licenseTime.LicenseTimeId))
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
            return View(licenseTime);
        }

        // GET: LicenseTimes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var licenseTime = await _context.LicenseTimes
                .FirstOrDefaultAsync(m => m.LicenseTimeId == id);
            if (licenseTime == null)
            {
                return NotFound();
            }

            return View(licenseTime);
        }

        // POST: LicenseTimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var licenseTime = await _context.LicenseTimes.FindAsync(id);
            _context.LicenseTimes.Remove(licenseTime);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LicenseTimeExists(int id)
        {
            return _context.LicenseTimes.Any(e => e.LicenseTimeId == id);
        }
    }
}
