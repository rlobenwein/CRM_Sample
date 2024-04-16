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
    public class TransactionCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Name,Notes,IsExpense";

        public TransactionCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransactionCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.TransactionCategories.OrderBy(x => x.IsExpense).ThenBy(x => x.Name).ToListAsync());
        }

        // GET: TransactionCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionCategories == null)
            {
                return NotFound();
            }

            var transactionCategory = await _context.TransactionCategories
                .Include(x => x.Subcategories).ThenInclude(x => x.TransactionType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionCategory == null)
            {
                return NotFound();
            }

            return View(transactionCategory);
        }

        // GET: TransactionCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransactionCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] TransactionCategory transactionCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transactionCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transactionCategory);
        }

        // GET: TransactionCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionCategories == null)
            {
                return NotFound();
            }

            var transactionCategory = await _context.TransactionCategories.FindAsync(id);
            if (transactionCategory == null)
            {
                return NotFound();
            }
            return View(transactionCategory);
        }

        // POST: TransactionCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] TransactionCategory transactionCategory)
        {
            if (id != transactionCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionCategoryExists(transactionCategory.Id))
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
            return View(transactionCategory);
        }

        // GET: TransactionCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TransactionCategories == null)
            {
                return NotFound();
            }

            var transactionCategory = await _context.TransactionCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionCategory == null)
            {
                return NotFound();
            }
            bool trExists = _context.Transactions.Include(x => x.TransactionType).ThenInclude(x => x.Subcategory).Any(x => x.TransactionType.Subcategory.CategoryId == id);
            if (trExists)
            {
                ModelState.AddModelError("Erro", "Existem uma ou mais transações cadastradas nesta Categoria. Exclua ou edite as transações para prosseguir.");
                ViewData["Error"] = true;
                return View(transactionCategory);
            }
            ViewData["Error"] = false;
            return View(transactionCategory);
        }

        // POST: TransactionCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TransactionCategories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TransactionCategory'  is null.");
            }
            var transactionCategory = await _context.TransactionCategories.FindAsync(id);
            if (transactionCategory != null)
            {
                _context.TransactionCategories.Remove(transactionCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionCategoryExists(int id)
        {
            return _context.TransactionCategories.Any(e => e.Id == id);
        }
    }
}
