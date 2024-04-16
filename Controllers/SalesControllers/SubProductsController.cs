using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Data;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class SubProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id,Name,Id,Description,Price,Currency";

        public SubProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Subproducts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SubProducts.Include(s => s.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Subproducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subProduct = await _context.SubProducts
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subProduct == null)
            {
                return NotFound();
            }

            return View(subProduct);
        }

        // GET: Subproducts/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Subproducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] SubProduct subProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subProduct);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name", subProduct.ProductId);
            return View(subProduct);
        }

        // GET: Subproducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subProduct = await _context.SubProducts.FindAsync(id);
            if (subProduct == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name", subProduct.ProductId);
            return View(subProduct);
        }

        // POST: Subproducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] SubProduct subProduct)
        {
            if (id != subProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubProductExists(subProduct.Id))
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
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name", subProduct.ProductId);
            return View(subProduct);
        }

        // GET: Subproducts/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subProduct = await _context.SubProducts
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subProduct == null)
            {
                return NotFound();
            }

            return View(subProduct);
        }

        // POST: Subproducts/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subProduct = await _context.SubProducts.FindAsync(id);
            _context.SubProducts.Remove(subProduct);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SubProductExists(int id)
        {
            return _context.SubProducts.Any(e => e.Id == id);
        }
    }
}
