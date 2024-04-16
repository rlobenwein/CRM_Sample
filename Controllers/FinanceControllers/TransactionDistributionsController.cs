using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using RLBW_ERP.Models.SalesModels;
using RLBW_ERP.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    [Authorize(Roles = "SuperAdmin,Director")]
    public class TransactionDistributionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,CostCenterId,TransactionId,Proportion,Notes";
        public TransactionDistributionsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = await _context.TransactionDistributions
                .Include(t => t.CostCenter)
                .Include(x => x.Transaction)
                .ToListAsync();
            return View(applicationDbContext);
        }

        // GET: TransactionDistributions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionDistributions == null)
            {
                return NotFound();
            }

            var transactionDistribution = await _context.TransactionDistributions
                .Include(t => t.CostCenter)
                .Include(x => x.Transaction)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionDistribution == null)
            {
                return NotFound();
            }

            return View(transactionDistribution);
        }
        public async Task<IActionResult> DetailsByTransaction(int? transactionId)
        {
            if (transactionId == null) return NotFound();

            var transaction = await _context.Transactions
                .Include(x => x.TransactionType).ThenInclude(x => x.Subcategory).ThenInclude(x => x.Category)
                .AsNoTracking().FirstAsync(x => x.Id == transactionId);

            var dist = await (from td in _context.TransactionDistributions.Where(x => x.TransactionId == transactionId)
                .Include(x => x.CostCenter).ThenInclude(x => x.Opportunity).ThenInclude(x => x.Company)
                              select new TransactionDistributionViewModel()
                              {
                                  Id = td.Id,
                                  CostCenterId = td.CostCenterId,
                                  OpportunityId = td.CostCenter.OpportunityId,
                                  OpportunityTitle = td.CostCenter.Opportunity.Title,
                                  CompanyName = td.CostCenter.Opportunity.Company.FriendlyName,
                                  Proportion = td.Proportion,
                                  Notes = td.Notes
                              }
                ).AsNoTracking().ToListAsync();

            var trDist = new TransactionDistributionDetailsViewModel()
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Category = transaction.TransactionType.Subcategory.Category.Name,
                Subcategory = transaction.TransactionType.Subcategory.Name,
                Type = transaction.TransactionType.Name,
                Description = transaction.Description,
                Value = transaction.Value,
                Notes = transaction.Notes,
                Document = transaction.Document,
                Distribution = dist
            };

            return View(trDist);
        }

        // GET: TransactionDistributions/Create
        public IActionResult CreateByCostCenter(int costCenterId)
        {
            var transactionDistributions = _context.TransactionDistributions
                                                .Include(x => x.Transaction)
                                                    .ThenInclude(x => x.TransactionType)
                                                        .ThenInclude(x => x.Subcategory)
                                                            .ThenInclude(x => x.Category)
                                                .Where(x => x.CostCenterId == costCenterId)
                                                .ToList();
            var transactions = from t in _context.Transactions
                                    .Where(t => t.Distributions.Sum(x => x.Proportion) < 1)
                                    .Include(x => x.TransactionType)
                                        .ThenInclude(x => x.Subcategory)
                                            .ThenInclude(x => x.Category)
                                    .AsNoTracking()
                               select new TransactionToDistributeViewModel
                               {
                                   Id = t.Id,
                                   Date = t.Date,
                                   Category = t.TransactionType.Subcategory.Category.Name,
                                   Subcategory = t.TransactionType.Subcategory.Name,
                                   Type = t.TransactionType.Name,
                                   Description = t.Description,
                                   Value = t.Value
                               };

            var model = new CostCenterCreateTransactionViewModel()
            {
                CostCenterId = costCenterId,
                TransactionDistributions = transactionDistributions,
                Transactions = transactions.ToList()
            };

            ViewData["TransactionId"] = new SelectList(_context.Transactions, "Id", "Description");
            return View(model);
        }

        // POST: TransactionDistributions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] CostCenterCreateTransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                _context.TransactionDistributions.UpdateRange(model.TransactionDistributions);

                foreach (var item in model.Transactions)
                {
                    if (item.Proportion == 0) continue;
                    var td = new TransactionDistribution
                    {
                        CostCenterId = model.CostCenterId,
                        TransactionId = item.Id,
                        Proportion = item.Proportion
                    };
                    _context.TransactionDistributions.Add(td);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "CostCenters", new { id = model.CostCenterId });
            }
            ViewData["CostCenterId"] = new SelectList(_context.CostCenters, "Id", "Notes", model.CostCenterId);
            return View(model);
        }

        public IActionResult CreateByTransaction(int transactionId)
        {
            var model = new TransactionDistribution()
            {
                TransactionId = transactionId,
                Transaction = _context.Transactions.Find(transactionId),
            };
            var costCenters = (from c in _context.CostCenters
                              .Include(x => x.Opportunity).ThenInclude(x => x.Company)
                              .Include(x => x.Opportunity).ThenInclude(x => x.Product).ThenInclude(x => x.Category)
                               select new
                               {
                                   c.Id,
                                   c.OpportunityId,
                                   c.Opportunity.Company.FriendlyName,
                                   c.Opportunity.Title,
                                   c.Opportunity.Product.Category.CategoryName,
                                   c.Opportunity.Product.Name,
                                   DisplayField = string.Format("CC {0} (Op.: {1} - {2} | Cliente: {3})",
                                                                 c.Id,
                                                                 c.OpportunityId,
                                                                 c.Opportunity.Title ?? c.Opportunity.Product.Category.CategoryName +
                                                                 " " +
                                                                 c.Opportunity.Product.Name,
                                                                 c.Opportunity.Company.FriendlyName)
                               }).AsNoTracking().ToList();

            ViewData["CostCenterId"] = new SelectList(costCenters, "Id", "DisplayField");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateByTransaction([Bind(BIND_STRING)] TransactionDistribution model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "CostCenters", new { id = model.CostCenterId });
            }
            var costCenters = (from c in _context.CostCenters
                              .Include(x => x.Opportunity).ThenInclude(x => x.Company)
                              .Include(x => x.Opportunity).ThenInclude(x => x.Product).ThenInclude(x => x.Category)
                               select new
                               {
                                   c.Id,
                                   c.OpportunityId,
                                   c.Opportunity.Company.FriendlyName,
                                   c.Opportunity.Title,
                                   c.Opportunity.Product.Category.CategoryName,
                                   c.Opportunity.Product.Name,
                                   DisplayField = string.Format("CC {0} (Op.: {1} - {2} | Cliente: {3})",
                                                                 c.Id, 
                                                                 c.OpportunityId,
                                                                 c.Opportunity.Title ?? c.Opportunity.Product.Category.CategoryName +
                                                                 " " +
                                                                 c.Opportunity.Product.Name,
                                                                 c.Opportunity.Company.FriendlyName)
                               }).AsNoTracking().ToList();

            ViewData["CostCenterId"] = new SelectList(costCenters, "CostCenterId", "DisplayField");
            return View(model);
        }

        // GET: TransactionDistributions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionDistributions == null)
            {
                return NotFound();
            }

            var transactionDistribution = await _context.TransactionDistributions.FindAsync(id);
            if (transactionDistribution == null)
            {
                return NotFound();
            }
            ViewData["TransactionId"] = new SelectList(_context.Transactions, "Id", "Description", transactionDistribution.TransactionId);
            ViewData["CostCenterId"] = new SelectList(_context.CostCenters, "Id", "Notes", transactionDistribution.CostCenterId);
            return View(transactionDistribution);
        }

        // POST: TransactionDistributions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] TransactionDistribution transactionDistribution)
        {
            if (id != transactionDistribution.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionDistribution);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionDistributionExists(transactionDistribution.Id))
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
            ViewData["TransactionId"] = new SelectList(_context.Transactions, "Id", "Description", transactionDistribution.TransactionId);
            ViewData["CostCenterId"] = new SelectList(_context.CostCenters, "Id", "Notes", transactionDistribution.CostCenterId);
            return View(transactionDistribution);
        }

        // GET: TransactionDistributions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TransactionDistributions == null)
            {
                return NotFound();
            }

            var transactionDistribution = await _context.TransactionDistributions
                .Include(t => t.CostCenter)
                .Include(x => x.Transaction)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionDistribution == null)
            {
                return NotFound();
            }

            return View(transactionDistribution);
        }

        // POST: TransactionDistributions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TransactionDistributions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TransactionDistribution'  is null.");
            }
            var transactionDistribution = await _context.TransactionDistributions.FindAsync(id);
            if (transactionDistribution != null)
            {
                _context.TransactionDistributions.Remove(transactionDistribution);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionDistributionExists(int id)
        {
            return _context.TransactionDistributions.Any(e => e.Id == id);
        }
    }
}
