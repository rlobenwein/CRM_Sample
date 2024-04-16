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
    public class TransactionSubcategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Name,Notes,Id";


        public TransactionSubcategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransactionSubcategories
        public async Task<IActionResult> Index()
        {
            var model = await _context.TransactionSubcategories.Include(t => t.Category).OrderBy(x => x.Category.Name).ThenBy(x => x.Name).ToListAsync();
            return View(model);
        }

        // GET: TransactionSubcategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionSubcategories == null)
            {
                return NotFound();
            }

            var transactionSubcategory = await _context.TransactionSubcategories
                .Include(t => t.Category)
                .Include(x => x.TransactionType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionSubcategory == null)
            {
                return NotFound();
            }

            return View(transactionSubcategory);
        }

        // GET: TransactionSubcategories/Create
        public IActionResult Create(int? id)
        {
            if (id != null)
            {
                ViewData["Id"] = new SelectList(_context.TransactionCategories, "Id", "Name", id);
                ViewData["IsExpense"] = _context.TransactionCategories.Find(id).IsExpense;
                return View();
            }
            ViewData["Id"] = new SelectList(_context.TransactionCategories.Where(x => x.IsExpense), "Id", "Name");
            return View();
        }

        // POST: TransactionSubcategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] TransactionSubcategory transactionSubcategory)
        {
            if (transactionSubcategory.Id > 0) transactionSubcategory.Id = new int();
            if (ModelState.IsValid)
            {
                _context.Add(transactionSubcategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.TransactionCategories, "Id", "Name", transactionSubcategory.CategoryId);
            return View(transactionSubcategory);
        }

        // GET: TransactionSubcategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionSubcategories == null)
            {
                return NotFound();
            }

            var transactionSubcategory = await _context.TransactionSubcategories.FindAsync(id);
            if (transactionSubcategory == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.TransactionCategories, "Id", "Name", transactionSubcategory.CategoryId);
            return View(transactionSubcategory);
        }

        // POST: TransactionSubcategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] TransactionSubcategory transactionSubcategory)
        {
            if (id != transactionSubcategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionSubcategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionSubcategoryExists(transactionSubcategory.Id))
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
            ViewData["Id"] = new SelectList(_context.TransactionCategories, "Id", "Name", transactionSubcategory.CategoryId);
            return View(transactionSubcategory);
        }

        // GET: TransactionSubcategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TransactionSubcategories == null)
            {
                return NotFound();
            }

            var transactionSubcategory = await _context.TransactionSubcategories
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionSubcategory == null)
            {
                return NotFound();
            }
            bool trExists = _context.Transactions.Include(x => x.TransactionType).Any(x => x.TransactionType.SubcategoryId == id);
            if (trExists)
            {
                ModelState.AddModelError("Erro", "Existem uma ou mais trasações cadastradas nesta Subcategoria. Exclua ou edite as transações para prosseguir.");
                ViewData["Error"] = true;
                return View(transactionSubcategory);
            }
            ViewData["Error"] = false;
            return View(transactionSubcategory);
        }

        // POST: TransactionSubcategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TransactionSubcategories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TransactionSubcategory'  is null.");
            }
            var transactionSubcategory = await _context.TransactionSubcategories.FindAsync(id);
            if (transactionSubcategory != null)
            {
                _context.TransactionSubcategories.Remove(transactionSubcategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionSubcategoryExists(int id)
        {
            return _context.TransactionSubcategories.Any(e => e.Id == id);
        }
    }
}
