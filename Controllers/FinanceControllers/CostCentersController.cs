using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    public class CostCentersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Id,Notes,Status";

        public CostCentersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CostCenters
        public async Task<IActionResult> Index()
        {
            var model = await _context.CostCenters
                .Include(c => c.Opportunity)
                    .ThenInclude(x => x.Company)
                .Include(x => x.TransactionsDistribution)
                    .ThenInclude(x => x.Transaction)
                .ToListAsync();

            return View(model);
        }

        // GET: CostCenters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CostCenters == null)
            {
                return NotFound();
            }

            var costCenter = await _context.CostCenters
                .Include(c => c.Opportunity)
                    .ThenInclude(x => x.Company)
                .Include(x => x.TransactionsDistribution)
                    .ThenInclude(x => x.Transaction)
                        .ThenInclude(x => x.TransactionType)
                            .ThenInclude(x => x.Subcategory)
                                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (costCenter == null)
            {
                return NotFound();
            }
            return View(costCenter);
        }

        // GET: CostCenters/Create
        public IActionResult Create(int? id)
        {
            if (id != null)
            {
                var model = new CostCenter()
                {
                    OpportunityId = (int)id,
                    IsActive = true,
                    Status = true
                };
                return PartialView(model);
            }
            return PartialView();
        }

        // POST: CostCenters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id")] int id)
        {
            if(id==0) return BadRequest();
            var costCenter = new CostCenter()
            {
                IsActive = true,
                Status = true,
                OpportunityId = id
            };
            if (ModelState.IsValid)
            {
                _context.Add(costCenter);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Opportunities", new { id = id });
            }
            ViewData["Id"] = new SelectList(_context.Opportunities, "Id", "Id", costCenter.OpportunityId);
            return View(costCenter);
        }

        // GET: CostCenters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CostCenters == null)
            {
                return NotFound();
            }

            var costCenter = await _context.CostCenters.FindAsync(id);
            if (costCenter == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Opportunities, "Id", "Id", costCenter.OpportunityId);
            return View(costCenter);
        }

        // POST: CostCenters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] CostCenter costCenter)
        {
            if (id != costCenter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(costCenter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CostCenterExists(costCenter.Id))
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
            ViewData["Id"] = new SelectList(_context.Opportunities, "Id", "Id", costCenter.OpportunityId);
            return View(costCenter);
        }

        // GET: CostCenters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CostCenters == null)
            {
                return NotFound();
            }

            var costCenter = await _context.CostCenters
                .Include(c => c.Opportunity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (costCenter == null)
            {
                return NotFound();
            }

            return View(costCenter);
        }

        // POST: CostCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CostCenters == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CostCenter'  is null.");
            }
            var costCenter = await _context.CostCenters.FindAsync(id);
            if (costCenter != null)
            {
                _context.CostCenters.Remove(costCenter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CostCenterExists(int id)
        {
            return _context.CostCenters.Any(e => e.Id == id);
        }
    }
}
