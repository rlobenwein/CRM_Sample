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
    public class PaymentAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PaymentAccounts
        public async Task<IActionResult> Index()
        {
              return View(await _context.PaymentAccounts.ToListAsync());
        }

        // GET: PaymentAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PaymentAccounts == null)
            {
                return NotFound();
            }

            var paymentAccount = await _context.PaymentAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentAccount == null)
            {
                return NotFound();
            }

            return View(paymentAccount);
        }

        // GET: PaymentAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Notes")] PaymentAccount paymentAccount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paymentAccount);
        }

        // GET: PaymentAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PaymentAccounts == null)
            {
                return NotFound();
            }

            var paymentAccount = await _context.PaymentAccounts.FindAsync(id);
            if (paymentAccount == null)
            {
                return NotFound();
            }
            return View(paymentAccount);
        }

        // POST: PaymentAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Notes")] PaymentAccount paymentAccount)
        {
            if (id != paymentAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentAccountExists(paymentAccount.Id))
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
            return View(paymentAccount);
        }

        // GET: PaymentAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PaymentAccounts == null)
            {
                return NotFound();
            }

            var paymentAccount = await _context.PaymentAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentAccount == null)
            {
                return NotFound();
            }

            return View(paymentAccount);
        }

        // POST: PaymentAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PaymentAccounts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PaymentAccount'  is null.");
            }
            var paymentAccount = await _context.PaymentAccounts.FindAsync(id);
            if (paymentAccount != null)
            {
                _context.PaymentAccounts.Remove(paymentAccount);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentAccountExists(int id)
        {
          return _context.PaymentAccounts.Any(e => e.Id == id);
        }
    }
}
