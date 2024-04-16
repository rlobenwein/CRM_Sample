using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.FinanceModels;
using RLBW_ERP.Models.SalesModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP.Controllers.FinanceControllers
{
    public class PaymentTermsController : Controller
    {
        private const string BIND_STRING = "Id,Name,Description";
        private readonly ApplicationDbContext _context;

        public PaymentTermsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: PaymentTermsController
        public ActionResult Index()
        {
            var data = _context.PaymentTerms.ToList();
            return View(data);
        }

        // GET: PaymentTermsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var data = await _context.PaymentTerms.FindAsync(id);
            return View(data);
        }

        // GET: PaymentTermsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PaymentTermsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(BIND_STRING)] PaymentTerm paymentTerm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(paymentTerm);
                    await _context.SaveChangesAsync();

                    var referer = Request.Headers["Referer"].ToString();
                    if (referer != null)
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return View(paymentTerm);
            }
            return View(paymentTerm);
        }

        // GET: PaymentTermsController/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.PaymentTerms.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        // POST: PaymentTermsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind(BIND_STRING)] PaymentTerm paymentTerm)
        {
            if (id != paymentTerm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentTerm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(paymentTerm.Id))
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
            return View(paymentTerm);
        }

        // GET: PaymentTermsController/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.PaymentTerms.FindAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: PaymentTermsController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.PaymentTerms.FindAsync(id);
            _context.PaymentTerms.Remove(model);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.PaymentTerms.Any(e => e.Id == id);
        }
    }
}

