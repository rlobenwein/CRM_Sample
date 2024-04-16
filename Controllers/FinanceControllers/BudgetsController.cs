using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using RLBW_ERP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    [Authorize(Roles = "SuperAdmin,Director")]
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Year,Frequency,TransactionCategoryId,TransactionSubcategoryId,TransactionTypeId,TransactionType,TotalBudgeted,Description,TransactionDay,InitialMonth";
        private const string BIND_STRING_VM = "Year,TotalBudget,Transactions";

        public BudgetsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: BudgetsController
        public ActionResult Index()
        {
            var years = _context.Budgets.Select(x => x.Year).Distinct().ToList();
            List<BudgetIndexViewModel> budgets = new();
            foreach (var item in years)
            {
                budgets.Add(new BudgetIndexViewModel { Year = item, BudgetItems = _context.Budgets.Where(x => x.Year == item).ToList() });
            }

            return View(budgets);
        }

        // GET: BudgetsController/Details/5
        public async Task<ActionResult> Details(int year)
        {
            var model = new BudgetIndexViewModel
            {
                Year = year,
                BudgetItems = await _context.Budgets.Where(x => x.Year == year)
                .Include(x => x.TransactionType)
                    .ThenInclude(x => x.Subcategory)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Transactions)
                .ToListAsync(),
            };

            return View(model);
        }

        // GET: BudgetsController/Create
        public ActionResult Create(int year)
        {
            ViewData["Id"] = new SelectList(_context.TransactionCategories.Where(x => x.IsExpense).AsEnumerable(), "Id", "Name");
            if (year > 0)
            {
                var model = new Budget() { Year = year };
                return View(model);
            };
            return View();
        }

        // POST: BudgetsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(BIND_STRING)] Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Add(budget);
                await _context.SaveChangesAsync();

                if (budget.Id > 0)
                {
                    CreateTransactions(budget);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { year = budget.Year });
                }
                return RedirectToAction("Index");
            }
            return View(budget);
        }

        private void CreateTransactions(Budget budget)
        {
            DateTime firstDate = SetFirstDate(budget);
            for (int i = 0; i < budget.Frequency; i++)
            {
                var transaction = new Transaction()
                {
                    Date = firstDate.AddMonths(i),
                    Description = budget.Description,
                    IsExpense = true,
                    IsIncurred = false,
                    TypeId = budget.TransactionTypeId,
                    TransactionType = budget.TransactionType,
                    Value = budget.TotalBudgeted,
                    PaymentAccountId = 1,
                    BudgetId = budget.Id,
                    BankTransactionId = $"Budget_{budget.Year}_({Guid.NewGuid().ToString()[..8]})",
                };
                _context.Add(transaction);
            }
        }

        private static DateTime SetFirstDate(Budget budget)
        {
            int maxDay = DateTime.DaysInMonth(budget.Year, budget.InitialMonth ?? 1);
            int transactionDay = budget.TransactionDay ?? 1;
            if (transactionDay > maxDay)
            {
                transactionDay = maxDay;
            }
            var firstDate = new DateTime(budget.Year, budget.InitialMonth ?? 1, transactionDay).Date;
            return firstDate;
        }

        // GET: BudgetsController/Edit/5
        public ActionResult Edit(int id)
        {
            var model = _context.Budgets.Include(x => x.TransactionType)
                .ThenInclude(x => x.Subcategory)
                .ThenInclude(x => x.Category)
                .FirstOrDefault(x => x.Id == id);
            ViewData["Id"] = new SelectList(_context.TransactionCategories.Where(x => x.IsExpense).AsEnumerable(), "Id", "Name", model.TransactionType.Subcategory.CategoryId);
            ViewData["SubcategoryId"] = new SelectList(_context.TransactionSubcategories.Where(x => x.CategoryId == model.TransactionType.Subcategory.CategoryId).AsEnumerable(), "Id", "Name", model.TransactionType.SubcategoryId);
            ViewData["TypeId"] = new SelectList(_context.TransactionTypes.Where(x => x.SubcategoryId == model.TransactionType.SubcategoryId).AsEnumerable(), "Id", "Name", model.TransactionTypeId);

            return View(model);
        }

        // POST: BudgetsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind(BIND_STRING)] Budget budget)
        {
            if (budget.Id != id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                UpdateTransactions(budget);
                _context.Update(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Budgets", new { year = budget.Year });
            }
            return View(budget);
        }

        private void UpdateTransactions(Budget budget)
        {
            var transactions = _context.Transactions.Where(x => x.BudgetId == budget.Id);
            if (transactions.Count() != budget.Frequency)
            {
                if (transactions.Any()) _context.Transactions.RemoveRange(transactions);
                CreateTransactions(budget);
                return;
            }
            int i = 0;
            DateTime firstDate = SetFirstDate(budget);
            foreach (var t in transactions)
            {
                t.Date = firstDate.AddMonths(i++);
                t.Description = budget.Description;
                t.IsExpense = true;
                t.IsIncurred = false;
                t.TypeId = budget.TransactionTypeId;
                t.TransactionType = budget.TransactionType;
                t.Value = budget.TotalBudgeted;
                _context.Update(t);
            }
        }
        public async Task<IActionResult> UpdateTransactionsByBudgetId(int id)
        {
            var budget = _context.Budgets.Find(id);
            UpdateTransactions(budget);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: BudgetsController/Delete/5
        public ActionResult Delete(int id)
        {
            var model = _context.Budgets.Include(x => x.TransactionType)
                .ThenInclude(x => x.Subcategory)
                .ThenInclude(x => x.Category)
                .Include(x => x.Transactions)
                .FirstOrDefault(x => x.Id == id);
            return View(model);
        }

        // POST: BudgetsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var budget = _context.Budgets.Find(id);
            var transactions = _context.Transactions.Where(x => x.BudgetId == id);
            _context.Remove(budget);
            _context.RemoveRange(transactions);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { year = budget.Year });
        }
        // GET: BudgetsController/Delete/5
        public ActionResult DeleteRange(int year)
        {
            var model = new BudgetIndexViewModel
            {
                Year = year,
                BudgetItems = _context.Budgets.Where(x => x.Year == year)
                .Include(x => x.TransactionType)
                    .ThenInclude(x => x.Subcategory)
                    .ThenInclude(x => x.Category)
                .ToList()
            };

            return View(model);
        }

        // POST: BudgetsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteRange")]
        public async Task<ActionResult> DeleteRangeConfirmed(int year)
        {
            var budget = _context.Budgets.Where(x => x.Year == year);
            _context.RemoveRange(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult CreateBudget()
        {
            var yearsAvailable = _context.Budgets.Select(x => x.Year).Distinct().ToList();
            ViewData["YearsAvailable"] = new SelectList(yearsAvailable);
            ViewData["MinYear"] = yearsAvailable.Max() + 1;
            return PartialView();
        }
        //[HttpPost]
        public async Task<IActionResult> BudgetBasedOnPreviousYear(int yearReference, decimal amountIncrease, int yearToBudget)
        {
            amountIncrease /= 100;
            if (yearToBudget == 0) yearToBudget = 2025;
            if (amountIncrease == 0) amountIncrease = 0.1m;
            var previousBudget = await _context.Budgets.Where(x => x.Year == yearReference)
                .Include(x => x.TransactionType)
                    .ThenInclude(x => x.Subcategory)
                    .ThenInclude(x => x.Category)
                .ToListAsync();
            var newBudget = new List<Budget>();
            foreach (var item in previousBudget)
            {
                item.TotalBudgeted *= 1 + amountIncrease;
                item.Year = yearToBudget;
                newBudget.Add(item);
            }
            ViewData["YearReference"] = yearReference;
            ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Set<TransactionCategory>(), "Id", "Name");
            ViewData["SubcategoryId"] = new SelectList(_context.Set<TransactionSubcategory>(), "Id", "Name");
            return View(newBudget);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBudgetList(List<Budget> budgets)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.AddRange(budgets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(budgets);
        }
        public async Task<IActionResult> EditBudget(int year)
        {
            var budgets = await _context.Budgets.Where(x => x.Year == year)
                .Include(x => x.TransactionType)
                    .ThenInclude(x => x.Subcategory)
                    .ThenInclude(x => x.Category)
                .ToListAsync();
            ViewData["YearReference"] = year;
            ViewData["TypeId"] = new SelectList(_context.Set<TransactionType>(), "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Set<TransactionCategory>(), "Id", "Name");
            ViewData["SubcategoryId"] = new SelectList(_context.Set<TransactionSubcategory>(), "Id", "Name");
            return View(budgets);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEditedBudget(List<Budget> budgets)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.UpdateRange(budgets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(budgets);
        }
        //public async Task<IActionResult> ProcessTransactions(int year)
        //{
        //    ViewBag.ShowLoader = true;
        //    var model = new BudgetTransactionsViewModel
        //    {
        //        Year = year,
        //        Transactions = new List<Transaction>()
        //    };

        //    var budgets = await _context.Budgets.Where(x => x.Year == year)
        //        .Include(x => x.TransactionType)
        //        .ThenInclude(x => x.Subcategory)
        //        .ThenInclude(x => x.Category)
        //        .ToListAsync();
        //    int j = 0;
        //    foreach (var b in budgets)
        //    {
        //        for (int i = 0; i < b.Frequency; i++)
        //        {
        //            int maxDay = DateTime.DaysInMonth(b.Year, b.InitialMonth + i ?? 1);
        //            int transactionDay = b.TransactionDay ?? 1;
        //            if (transactionDay > maxDay)
        //            {
        //                transactionDay = maxDay;
        //            }
        //            var transaction = new Transaction()
        //            {
        //                Date = new DateTime(b.Year, b.InitialMonth + i ?? 1 + i, transactionDay).Date,
        //                Description = b.Description,
        //                IsExpense = true,
        //                IsIncurred = false,
        //                TypeId = b.TransactionTypeId,
        //                TransactionType = b.TransactionType,
        //                Value = b.TotalBudgeted,
        //                PaymentAccountId = 1,
        //                BudgetId = b.Id
        //            };
        //            if (TransactionsExists(transaction))
        //            {
        //                model.Transactions.AddRange(GetTransaction(transaction));
        //                continue;
        //            }
        //            model.Transactions.Add(transaction);
        //        }
        //    }
        //    ViewBag.ShowLoader = false;
        //    return View(model);
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ProcessTransactions([Bind(BIND_STRING_VM)] BudgetTransactionsViewModel model)
        //{
        //    foreach (var transaction in model.Transactions)
        //    {
        //        if (TransactionsExists(transaction))
        //        {
        //            var tr = GetTransaction(transaction).ToList();
        //            foreach (var item in tr)
        //            {
        //                item.Value = transaction.Value;
        //                _context.Update(item);
        //            }
        //        }
        //        else
        //        {
        //            transaction.BankTransactionId = $"Budget_{model.Year}_({Guid.NewGuid()})";
        //            transaction.PaymentAccountId = 1;
        //            _context.Add(transaction);
        //        }
        //    }
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        private IQueryable<Transaction> GetTransaction(Transaction t)
        {
            return _context.Transactions.Where(
                                    x => x.Date == t.Date &&
                                    x.IsExpense == t.IsExpense &&
                                    x.BudgetId == t.BudgetId &&
                                    x.IsIncurred == t.IsIncurred &&
                                    x.Description == t.Description &&
                                    x.TypeId == t.TypeId
                                    );
        }

        private bool TransactionsExists(Transaction t)
        {
            return _context.Transactions.Any(
                                            x => x.Date == t.Date &&
                                            x.IsExpense == t.IsExpense &&
                                            x.BudgetId == t.BudgetId &&
                                            x.IsIncurred == t.IsIncurred &&
                                            x.Description == t.Description &&
                                            x.TypeId == t.TypeId
                                            );

        }
    }
}
