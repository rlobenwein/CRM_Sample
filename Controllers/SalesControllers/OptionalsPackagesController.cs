using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.CustomerModels;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class OptionalsPackagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public OptionalsPackagesController(ApplicationDbContext context,IMemoryCache cache)
        {
            _cache = cache;
            _context = context;
        }

        // GET: OptionalsPackages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var optionalsPackage = await _context.OptionalsPackages
                .Include(o => o.LicenseOptional)
                .Include(o => o.SubproductsList)
                .FirstOrDefaultAsync(m => m.SubproductsListId == id);
            if (optionalsPackage == null)
            {
                return NotFound();
            }

            return View(optionalsPackage);
        }

        // GET: OptionalsPackages/Create
        public IActionResult Create(int id)
        {
            var optionalPackage = new OptionalsPackage()
            {
                SubproductsListId = id,
                Discount = 0,
                Quantity = 1
            };

            var subproductId = _context.SubproductsLists.FirstOrDefault(x => x.Id == id).SubproductId;

            ViewData["SubproductsListId"] = id;
            ViewData["OptionalId"] = new SelectList(GetOptionalsAvailable(subproductId, id), "LicenseOptionalId", "OptionalName");
            //ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id");
            return PartialView(optionalPackage);
        }

        // POST: OptionalsPackages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubproductsListId,OptionalId,Quantity,Discount")] OptionalsPackage optionalsPackage)
        {
            if (ModelState.IsValid)
            {
                Opportunity opportunity = CalcOpportunityValue(optionalsPackage, false);

                _context.Update(opportunity);
                _context.Add(optionalsPackage);
                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["OptionalId"] = new SelectList(_context.LicenseOptionals, "LicenseOptionalId", "OptionalName", optionalsPackage.OptionalId);
            ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id", optionalsPackage.SubproductsListId);
            return PartialView(optionalsPackage);
        }

        // GET: OptionalsPackages/Edit/5
        public async Task<IActionResult> Edit(int? subproductsListId, int? optionalId)
        {
            if (subproductsListId == null || optionalId == null)
            {
                return NotFound();
            }

            var optionalsPackage = await _context.OptionalsPackages.FindAsync(subproductsListId, optionalId);
            if (optionalsPackage == null)
            {
                return NotFound();
            }
            var subproductId = _context.SubproductsLists.FirstOrDefault(x => x.Id == subproductsListId).SubproductId;
            ViewData["OptionalId"] = new SelectList(GetOptionalsAvailable(subproductId, (int)subproductsListId, (int)optionalId), "LicenseOptionalId", "OptionalName", optionalsPackage.OptionalId);
            ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id", optionalsPackage.SubproductsListId);
            return PartialView(optionalsPackage);
        }

        // POST: OptionalsPackages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int subproductsListId, int optionalId, [Bind("SubproductsListId,OptionalId,Quantity,Discount")] OptionalsPackage optionalsPackage)
        {
            if (subproductsListId == optionalsPackage.SubproductsListId && optionalsPackage.OptionalId == optionalId)
            {
                if (ModelState.IsValid)
                {
                    Opportunity opportunity = CalcOpportunityValue(optionalsPackage, false);
                    try
                    {
                        _context.Update(opportunity);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OptionalsPackageExists(optionalsPackage.SubproductsListId, optionalsPackage.OptionalId))
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
                ViewData["OptionalId"] = new SelectList(_context.LicenseOptionals, "LicenseOptionalId", "OptionalName", optionalsPackage.OptionalId);
                ViewData["Id"] = new SelectList(_context.ProposalProducts, "Id", "Id", optionalsPackage.SubproductsListId);
                return PartialView(optionalsPackage);
            }
            return NotFound();
        }

        // GET: OptionalsPackages/Delete/5
        public async Task<IActionResult> Delete(int? subproductsListId, int? optionalId)
        {
            var optionalsPackage = await _context.OptionalsPackages
                .Include(o => o.LicenseOptional)
                .Include(o => o.SubproductsList)
                .FirstOrDefaultAsync(x => x.SubproductsListId == subproductsListId && x.OptionalId == optionalId);
            if (optionalsPackage == null)
            {
                return NotFound();
            }

            return PartialView(optionalsPackage);
        }

        // POST: OptionalsPackages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int subproductsListId, int optionalId)
        {
            var optionalsPackage = await _context.OptionalsPackages
                .FirstOrDefaultAsync(x => x.SubproductsListId == subproductsListId && x.OptionalId == optionalId);
            Opportunity opportunity = CalcOpportunityValue(optionalsPackage, true);

            _context.OptionalsPackages.Remove(optionalsPackage);
            _context.Update(opportunity);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
        }

        private bool OptionalsPackageExists(int subproductsListId, int optionalId)
        {
            return _context.OptionalsPackages.Any(e => e.SubproductsListId == subproductsListId && e.OptionalId == optionalId);
        }
        public JsonResult GetOptionalPrice(int optionalId)
        {
            var price = _context.LicenseOptionals.Find(optionalId).Price;
            return Json(price);
        }

        private List<LicenseOptional> GetOptionalsAvailable(int subproductId, int subproductsListId)
        {
            var optionalPackages = _context.OptionalsPackages.Where(x => x.SubproductsListId == subproductsListId).ToList();
            var allOptionals = _context.LicenseOptionals.Where(x => x.SubProductId == subproductId).ToList();
            List<LicenseOptional> optionalsAvailable = new();

            foreach (var item in allOptionals)
            {
                if (!optionalPackages.Any(x => x.OptionalId == item.LicenseOptionalId))
                {
                    optionalsAvailable.Add(item);
                }
            }

            return optionalsAvailable;
        }
        private List<LicenseOptional> GetOptionalsAvailable(int subproductId, int subproductsListId, int optionalId)
        {
            var optionalPackages = _context.OptionalsPackages.Where(x => x.SubproductsListId == subproductsListId);
            var allOptionals = _context.LicenseOptionals.Where(x => x.SubProductId == subproductId).ToList();
            List<LicenseOptional> optionalsAvailable = new();

            if (optionalPackages.Any())
            {
                foreach (var item in allOptionals)
                {
                    if (!optionalPackages.Any(x => x.OptionalId == item.LicenseOptionalId))
                    {
                        optionalsAvailable.Add(item);
                    }
                }
            }
            var optional = _context.LicenseOptionals.Find(optionalId);
            optionalsAvailable.Add(optional);

            return optionalsAvailable;
        }


        private Opportunity CalcOpportunityValue(OptionalsPackage optionalsPackage, bool delete)
        {
            var calcPrices = new Prices(_cache);
            SubproductsList subproductsListItem = _context.SubproductsLists.Include(x => x.Optionals).Include(x => x.Params).Include(x => x.ProposalProduct).FirstOrDefault(x => x.Id == optionalsPackage.SubproductsListId); 
            if (!delete)
            {
                optionalsPackage.BasePrice = _context.LicenseOptionals.Find(optionalsPackage.OptionalId).Price;
                optionalsPackage.LicenseOptional = _context.LicenseOptionals.Include(x => x.SubProduct).ThenInclude(x => x.Product).FirstOrDefault(x => x.LicenseOptionalId == optionalsPackage.OptionalId);
                var softwareParams = _context.SoftwareParams.Where(x => x.SubproductsListId == optionalsPackage.SubproductsListId).Include(x => x.SubproductsList).ThenInclude(x => x.ProposalProduct).FirstOrDefault();
                int software = optionalsPackage.LicenseOptional.SubProduct.ProductId;
                if (software == 4)
                {
                    List<OptionalsPackage> optionals = _context.OptionalsPackages.Include(x=>x.LicenseOptional).Where(x => x.SubproductsListId == optionalsPackage.SubproductsListId).ToList();

                    if (!optionals.Any(c => c.OptionalId == optionalsPackage.OptionalId)) optionals.Add(optionalsPackage);

                    optionals=calcPrices.SetOptionalsPrice(optionals, softwareParams, software,_context);
                    subproductsListItem.Optionals = optionals;
                }
                else
                {
                    calcPrices.SetOptionalsPrice(optionalsPackage, softwareParams, software);
                }
            }

            subproductsListItem.BasePrice = _context.SubProducts.FirstOrDefault(x => x.Id == subproductsListItem.SubproductId).Price;
            calcPrices.UpdateSubproductsListPrices(subproductsListItem, optionalsPackage, delete, true);

            ProposalProduct proposalProduct = _context.ProposalProducts.Include(x => x.Subproducts).FirstOrDefault(x => x.Id == subproductsListItem.ProposalProductId);
            calcPrices.UpdateProposalProductPrices(proposalProduct, subproductsListItem, false);

            Proposal proposal = _context.Proposals.Include(x => x.Products).ThenInclude(x => x.Product).FirstOrDefault(x => x.Id == proposalProduct.ProposalId);
            calcPrices.UpdateProposalPrices(proposal, proposalProduct, false);

            Opportunity opportunity = _context.Opportunities
                .Include(x => x.Proposals)
                .FirstOrDefault(x => x.Id == proposal.OpportunityId);
            calcPrices.CalcOpportunityValue(opportunity, proposal, false);

            return opportunity;
        }

    }
}
