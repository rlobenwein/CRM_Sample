using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class SubproductsListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string BIND_STRING= "Id,Id,SubproductId,Quantity";

        public SubproductsListsController(ApplicationDbContext context,IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: SubproductsLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subproductsList = await _context.SubproductsLists
                .Include(s => s.ProposalProduct)
                    .ThenInclude(x => x.Category)
                .Include(x => x.ProposalProduct.Product)
                .Include(x => x.Params)
                    .ThenInclude(x => x.LicenseNetworkType)
                .Include(x => x.Params)
                    .ThenInclude(x => x.LicenseTime)
                .Include(x => x.Params)
                    .ThenInclude(x => x.CommercialLicense)
                .Include(x => x.Optionals)
                .Include(s => s.SubProduct)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subproductsList == null)
            {
                return NotFound();
            }
            subproductsList.OptionalsList = GetOptionalsAvailable(subproductsList.SubproductId, subproductsList.Id);

            return PartialView(subproductsList);
        }
        // GET: SubproductsLists/Create
        public IActionResult Create(int productId, int proposalProductId)
        {
            var proposalProduct = _context.ProposalProducts
                .Include(x => x.Category)
                .Include(x => x.Product)
                .FirstOrDefault(x => x.Id == proposalProductId);

            var subproductItem = new SubproductsList()
            {
                ProposalProductId = proposalProductId,
                ProposalProduct = _context.ProposalProducts.Find(proposalProductId),
                Quantity = 1,
                Discount = 0
            };

            ViewData["SubproductId"] = new SelectList(GetSubproductsAvailable(productId, proposalProductId), "Id", "Name");
            return PartialView(subproductItem);
        }

        // POST: SubproductsLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] SubproductsList subproductsListItem)
        {
            if (ModelState.IsValid)
            {
                var productId = _context.SubProducts.FirstOrDefault(x => x.Id == subproductsListItem.SubproductId).ProductId;
                if (productId == 2)
                {
                    var softwareParams = new SoftwareParams
                    {
                        SubproductsListId = subproductsListItem.Id,
                        Tasks = 1,
                        Cores = 6,
                        Workbenchs = 0,
                        LicenseTimeId = 1,
                        LicenseNetworkId = 1,
                        CommercialLicenseId = 1,
                        TimeQuantity = 1,
                        Seats = 1,
                        Coefficient = 0.4m
                    };
                    subproductsListItem.Params = _context.SoftwareParams.Where(x => x.SubproductsListId == subproductsListItem.Id).ToList();
                    subproductsListItem.Params.Add(softwareParams);
                    _context.SoftwareParams.Add(softwareParams);
                }
                Opportunity opportunity = CalcOpportunityValue(subproductsListItem, false);

                _context.Update(opportunity);

                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id", subproductsListItem.ProposalProductId);
            ViewData["SubproductId"] = new SelectList(GetSubproductsAvailable(subproductsListItem.ProposalProduct.ProductId, subproductsListItem.ProposalProductId), "Id", "Name");

            return PartialView("Create", subproductsListItem);
        }

        // GET: SubproductsLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subproductsList = await _context.SubproductsLists
                .Include(x => x.SubProduct)
                .FirstOrDefaultAsync(x => x.Id == id);
            var proposalProduct = await _context.ProposalProducts.Where(x => x.Id == subproductsList.ProposalProductId)
                .Include(x => x.Category)
                .Include(x => x.Product)
                .FirstOrDefaultAsync();

            subproductsList.ProposalProduct = proposalProduct;
            subproductsList.Discount *= 100;

            if (subproductsList == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id", subproductsList.ProposalProductId);
            ViewData["SubproductId"] = new SelectList(GetSubproductsAvailable(subproductsList.SubProduct.ProductId, subproductsList.ProposalProductId, subproductsList.SubproductId), "Id", "Name");
            return PartialView(subproductsList);
        }

        // POST: SubproductsLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] SubproductsList subproductsListItem)
        {
            if (id != subproductsListItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                subproductsListItem.Params = _context.SoftwareParams.Where(x => x.SubproductsListId == subproductsListItem.Id).ToList();
                Opportunity opportunity = CalcOpportunityValue(subproductsListItem, false);
                try
                {
                    _context.Update(opportunity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubproductsListExists(subproductsListItem.Id))
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

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id", subproductsListItem.ProposalProductId);
            ViewData["SubproductId"] = new SelectList(GetSubproductsAvailable(subproductsListItem.SubProduct.ProductId, subproductsListItem.ProposalProductId), "Id", "Name");
            return PartialView(subproductsListItem);
        }

        // GET: SubproductsLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subproductsList = await _context.SubproductsLists
                .Include(s => s.ProposalProduct)
                .Include(s => s.SubProduct)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subproductsList == null)
            {
                return NotFound();
            }
            subproductsList.Discount *= 100;


            return PartialView(subproductsList);
        }

        // POST: SubproductsLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subproductsList = await _context.SubproductsLists.FindAsync(id);
            Opportunity opportunity = CalcOpportunityValue(subproductsList, true);

            _context.Update(opportunity);
            _context.SubproductsLists.Remove(subproductsList);

            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
        }

        private bool SubproductsListExists(int id)
        {
            return _context.SubproductsLists.Any(e => e.Id == id);
        }
        public JsonResult GetSubproductPrice(int subproductId)
        {
            if (subproductId == 0)
            {
                return Json(0);
            }
            var price = _context.SubProducts.Find(subproductId).Price;
            return Json(price);
        }
        private List<LicenseOptional> GetOptionalsAvailable(int subproductId, int subproductsListId)
        {
            var optionalPackages = _context.OptionalsPackages.Where(x => x.SubproductsListId == subproductsListId);
            var allOptionals = _context.LicenseOptionals.Where(x => x.SubProductId == subproductId).ToList();

            if (optionalPackages.Any())
            {
                foreach (var item in allOptionals)
                {
                    if (optionalPackages.Any(x => x.OptionalId == item.LicenseOptionalId))
                    {
                        item.Checked = true;
                    }
                }
            }

            return allOptionals;
        }


        private List<SubProduct> GetSubproductsAvailable(int productId, int proposalProductId, int subproductId)
        {
            var allSubProducts = _context.SubProducts.Where(x => x.ProductId == productId).ToList();
            var subproductsLists = _context.SubproductsLists.Where(x => x.ProposalProductId == proposalProductId).ToList();
            List<SubProduct> subproductsAvailable = new();

            foreach (var item in allSubProducts)
            {
                if (!subproductsLists.Any(x => x.SubproductId == item.Id))
                {
                    subproductsAvailable.Add(item);
                }
            }
            subproductsAvailable.Add(_context.SubProducts.Find(subproductId));

            return subproductsAvailable;
        }
        private List<SubProduct> GetSubproductsAvailable(int productId, int proposalProductId)
        {
            var allSubproducts = _context.SubProducts.Where(x => x.ProductId == productId).ToList();
            var subproductsLists = _context.SubproductsLists.Where(x => x.ProposalProductId == proposalProductId).ToList();
            List<SubProduct> subproductsAvailable = new();

            foreach (var item in allSubproducts)
            {
                if (!subproductsLists.Any(x => x.SubproductId == item.Id))
                {
                    subproductsAvailable.Add(item);
                }
            }
            subproductsAvailable.Insert(0, new SubProduct { Id = 0, Name = "Selecione" });

            return subproductsAvailable;
        }
        private Opportunity CalcOpportunityValue(SubproductsList subproductsListItem, bool delete)
        {
            subproductsListItem.Discount /= 100;
            var calcPrices = new Prices(_cache);
            if (!delete)
            {
                subproductsListItem.Optionals = _context.OptionalsPackages.Include(x => x.LicenseOptional).Where(x => x.SubproductsListId == subproductsListItem.Id).ToList();

                var dbSubproduct = _context.SubproductsLists.Find(subproductsListItem.Id);
                if (dbSubproduct != null && dbSubproduct.SubproductId != subproductsListItem.SubproductId)
                {
                    RemoveOptionals(subproductsListItem.Id);
                }
                subproductsListItem.BasePrice = _context.SubProducts.FirstOrDefault(x => x.Id == subproductsListItem.SubproductId).Price;
                if (subproductsListItem.Params == null || subproductsListItem.Params.Count < 1)
                {
                    subproductsListItem.Params = _context.SoftwareParams.Where(x => x.SubproductsListId == subproductsListItem.Id).ToList();
                }
                subproductsListItem.SubProduct = _context.SubProducts.Include(x => x.Product).FirstOrDefault(x => x.Id == subproductsListItem.SubproductId);
                calcPrices.SetSubproductListItemPrices(subproductsListItem);
            }

            ProposalProduct proposalProduct = _context.ProposalProducts.Include(x => x.Subproducts).FirstOrDefault(x => x.Id == subproductsListItem.ProposalProductId);
            calcPrices.UpdateProposalProductPrices(proposalProduct, subproductsListItem, delete);

            Proposal proposal = _context.Proposals.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefault(x => x.Id == proposalProduct.ProposalId);
            calcPrices.UpdateProposalPrices(proposal, proposalProduct, false);

            Opportunity opportunity = _context.Opportunities
                .Include(x => x.Proposals)
                .FirstOrDefault(x => x.Id == proposal.OpportunityId);
            calcPrices.CalcOpportunityValue(opportunity, proposal, false);

            return opportunity;
        }
        private void RemoveOptionals(int id)
        {
            var optionals = _context.OptionalsPackages
                .Where(x => x.SubproductsListId == id)
                .Include(x => x.LicenseOptional)
                .ToList();

            if (optionals.Any(x => x.LicenseOptional.SubProductId != id))
            {
                _context.RemoveRange(optionals);
            }
        }
    }
}
