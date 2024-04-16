using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using RLBW_ERP.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    public class TransactionTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Name,Notes,SubcategoryId";

        public TransactionTypesController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: TransactionTypes
        public async Task<IActionResult> Index()
        {
            var types = await _context.TransactionTypes
                .Include(t => t.Subcategory)
                    .ThenInclude(x => x.Category)
                .ToListAsync();
            return View(types);
        }

        // GET: TransactionTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionTypes == null)
            {
                return NotFound();
            }

            var transactionSubcategory = await (from tt in _context.TransactionTypes
                .Include(t => t.Subcategory).ThenInclude(x => x.Category)
                                                where tt.Id == id
                                                select new TransactionTypeViewModel
                                                {
                                                    Id = tt.Id,
                                                    Name = tt.Name,
                                                    Notes = tt.Notes,
                                                    SubcategoryId = tt.SubcategoryId,
                                                    SubcategoryName = tt.Subcategory.Name,
                                                    IsExpense = tt.Subcategory.Category.IsExpense ? "Despesa" : "Receita",
                                                    CategoryName = tt.Subcategory.Category.Name,
                                                    CategoryId = tt.Subcategory.CategoryId

                                                }).FirstAsync();

            transactionSubcategory.Transactions = await (from t in _context.Transactions
                                                        .Include(x => x.PaymentAccount)
                                                         where t.TypeId == id
                                                         select new TransactionsViewModel
                                                         {
                                                             Id = t.Id,
                                                             Date = t.Date,
                                                             Description = t.Description,
                                                             Value = t.Value,
                                                             PaymentAccountName = t.PaymentAccount.Name
                                                         }).ToListAsync();

            if (transactionSubcategory == null)
            {
                return NotFound();
            }

            return View(transactionSubcategory);
        }

        // GET: TransactionTypes/Create
        public IActionResult Create(int? id, bool? isExpense)
        {
            if (id != null)
            {
                var model = new TransactionType()
                {
                    SubcategoryId = id.Value
                };
                var categoryId = _context.TransactionSubcategories.Find(id).CategoryId;
                ViewData["IsExpense"] = isExpense;
                ViewData["SubcategoryId"] = new SelectList(_context.TransactionSubcategories.Where(x => x.CategoryId == categoryId), "Id", "Name", id);
                ViewData["Id"] = new SelectList(_context.TransactionCategories.Where(x => x.IsExpense == isExpense), "Id", "Name", categoryId);
                return View(model);
            }
            ViewData["SubcategoryId"] = new SelectList(string.Empty, "Id", "Name");
            ViewData["Id"] = new SelectList(_context.TransactionCategories.Where(x => x.IsExpense), "Id", "Name");
            return View();
        }

        // POST: TransactionTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] TransactionType transactionSubcategory)
        {
            if (transactionSubcategory.Id > 0) transactionSubcategory.Id = 0;
            if (ModelState.IsValid)
            {
                _context.Add(transactionSubcategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubcategoryId"] = new SelectList(_context.TransactionCategories, "Id", "Name", transactionSubcategory.SubcategoryId);
            return View(transactionSubcategory);
        }

        // GET: TransactionTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionTypes == null)
            {
                return NotFound();
            }

            var transactionType = await _context.TransactionTypes.FindAsync(id);
            if (transactionType == null)
            {
                return NotFound();
            }
            var subcategoryId = transactionType.SubcategoryId;
            var categoryId = _context.TransactionSubcategories.Find(subcategoryId).CategoryId;
            bool isExpense = _context.TransactionCategories.Find(categoryId).IsExpense;
            ViewData["IsExpense"] = isExpense;
            ViewData["SubcategoryId"] = new SelectList(_context.TransactionSubcategories.Where(x => x.CategoryId == categoryId), "Id", "Name", id);
            ViewData["Id"] = new SelectList(_context.TransactionCategories.Where(x => x.IsExpense == isExpense), "Id", "Name", categoryId);

            return View(transactionType);
        }

        // POST: TransactionTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] TransactionType transactionSubcategory)
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
                    if (!TransactionTypeExists(transactionSubcategory.Id))
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
            ViewData["SubcategoryId"] = new SelectList(_context.TransactionCategories, "Id", "Name", transactionSubcategory.SubcategoryId);
            return View(transactionSubcategory);
        }

        // GET: TransactionTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TransactionTypes == null)
            {
                return NotFound();
            }

            var trType = await _context.TransactionTypes
                .Include(t => t.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trType == null)
            {
                return NotFound();
            }
            bool trExists = _context.Transactions.Any(x => x.TypeId == id);
            if (trExists)
            {
                ModelState.AddModelError("Erro", "Existem uma ou mais trasações cadastradas com este Tipo. Exclua ou edite as transações para prosseguir.");
                ViewData["Error"] = true;
                return View(trType);
            }

            ViewData["Error"] = false;
            return View(trType);
        }

        // POST: TransactionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trType = await _context.TransactionTypes.FindAsync(id);
            bool trExists = _context.Transactions.Any(x => x.TypeId == id);
            if (trExists)
            {
                ModelState.AddModelError("Erro", "Existem uma ou mais transações cadastradas com este Tipo. Exclua ou edite as transações para prosseguir.");
            }

            if (trType != null && ModelState.IsValid)
            {
                _context.TransactionTypes.Remove(trType);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(trType);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionTypeExists(int id)
        {
            return _context.TransactionTypes.Any(e => e.Id == id);
        }
    }
}
