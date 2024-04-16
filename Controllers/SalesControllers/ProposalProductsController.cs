using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CRM_Sample.Common;
using CRM_Sample.Data;
using CRM_Sample.Models.SalesModels;

namespace CRM_Sample.Controllers.SalesControllers
{
    public class ProposalProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string BIND_STRING = "Id,ProposalId,CategoryId,ProductId,BasePrice,Price,Discount,ManualPrice";

        public ProposalProductsController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: ProposalProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proposalProduct = await _context.ProposalProducts
                    .Include(p => p.Category)
                    .Include(p => p.Product)
                    .Include(p => p.Proposal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proposalProduct == null)
            {
                return NotFound();
            }
            proposalProduct.Discount *= 100;
            return PartialView(proposalProduct);
        }

        // GET: ProposalProducts/Create
        public async Task<IActionResult> Create(int proposalId)
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            var proposalProduct = new ProposalProduct()
            {
                ProposalId = proposal.Id,
                Discount = 0
            };

            return PartialView(proposalProduct);
        }

        // POST: ProposalProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] ProposalProduct proposalProduct)
        {
            if (ModelState.IsValid)
            {
                var calcPrices = new Prices(_cache);
                Opportunity opportunity = await calcPrices.CalcOpportunityValueAsync(proposalProduct, _context, false);

                _context.Update(opportunity);
                _context.Add(proposalProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            CreateViewData(proposalProduct);
            return PartialView(proposalProduct);
        }

        // GET: ProposalProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proposalProduct = await _context.ProposalProducts.FindAsync(id);
            proposalProduct.Category = await _context.Categories.FindAsync(proposalProduct.CategoryId);
            proposalProduct.Product = await _context.Products.FindAsync(proposalProduct.ProductId);

            if (proposalProduct == null)
            {
                return NotFound();
            }
            CreateViewData(proposalProduct);

            proposalProduct.Discount *= 100;
            return PartialView(proposalProduct);
        }

        // POST: ProposalProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] ProposalProduct proposalProduct)
        {
            if (id != proposalProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var calcPrices = new Prices(_cache);
                Opportunity opportunity = await calcPrices.CalcOpportunityValueAsync(proposalProduct, _context, false);
                try
                {

                    _context.Update(opportunity);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProposalProductExists(proposalProduct.ProposalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            CreateViewData(proposalProduct);
            return PartialView(proposalProduct);
        }

        // GET: ProposalProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proposalProduct = await _context.ProposalProducts
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Proposal)
                .FirstOrDefaultAsync(m => m.Id == id);
            proposalProduct.Discount *= 100;
            if (proposalProduct == null)
            {
                return NotFound();
            }

            return PartialView(proposalProduct);
        }

        // POST: ProposalProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proposalProduct = await _context.ProposalProducts.FindAsync(id);
            var calcPrices = new Prices(_cache);
            
            Opportunity opportunity = await calcPrices.CalcOpportunityValueAsync(proposalProduct, _context, true);

            _context.Update(opportunity);
            _context.ProposalProducts.Remove(proposalProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
        }

        private bool ProposalProductExists(int id)
        {
            return _context.ProposalProducts.Any(e => e.ProductId == id);
        }

        public JsonResult GetProductList(int categoryId)
        {
            List<Product> productList = (from products in _context.Products
                                         where products.CategoryId == categoryId
                                         select products).ToList();

            productList.Insert(0, new Product { Id = 0, Name = "Selecione" });

            return Json(new SelectList(productList, "Id", "Name"));
        }
        public async Task<JsonResult> GetProposalProductItems(int opportunityProductId)
        {
            var proposalProduct = await _context.ProposalProducts
                .FirstOrDefaultAsync(o => o.ProposalId == opportunityProductId);
            var category = proposalProduct.Product.Category.CategoryName;
            var product = proposalProduct.Product.Name;

            return Json($"{category} {product}");
        }
        private void CreateViewData(ProposalProduct proposalProduct=null)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", proposalProduct?.CategoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", proposalProduct?.ProductId);
            ViewData["ProposalId"] = new SelectList(_context.Proposals, "Id", "Id", proposalProduct?.ProposalId);
        }

    }

}
