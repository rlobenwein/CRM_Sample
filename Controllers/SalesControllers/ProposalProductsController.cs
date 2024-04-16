using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class ProposalProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string BIND_STRING = "Id,Id,Id,Id,BasePrice,Price,Discount,ManualPrice";

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
                    .Include(p => p.Subproducts)
                        .ThenInclude(x => x.SubProduct)
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
            var net = _context.LicenseNetworkTypes.ToList();

            List<Category> category = (from categories in _context.Categories
                                       select categories).ToList();
            category.Insert(0, new Category { Id = 0, CategoryName = "Selecione" });

            ViewData["Id"] = new SelectList(category, "Id", "CategoryName");
            ViewData["Id"] = new SelectList(_context.Proposals, "Id", "Id");

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

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }

            List<Category> category = (from categories in _context.Categories
                                       select categories).ToList();
            category.Insert(0, new Category { Id = 0, CategoryName = "Selecione" });

            ViewData["Id"] = Json(new SelectList(category, "Id", "CategoryName"));
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time");
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType");
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type");
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Proposals, "Id", "Id");
            ViewData["SubproductId"] = new SelectList(_context.SubProducts, "Id", "Name");
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
            ViewData["Id"] = new SelectList(_context.Categories, "Id", "CategoryName");
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time");
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType");
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type");
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Proposals, "Id", "Id");
            ViewData["SubproductId"] = new SelectList(_context.SubProducts, "Id", "Name");

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
                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["Id"] = new SelectList(_context.Categories, "Id", "CategoryName");
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time");
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType");
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type");
            ViewData["Id"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Proposals, "Id", "Id");
            ViewData["SubproductId"] = new SelectList(_context.SubProducts, "Id", "Name");
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

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

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
        public JsonResult GetSubProductList(int productId)
        {
            List<SubProduct> subProductList = (from subproducts in _context.SubProducts
                                               where subproducts.ProductId == productId
                                               select subproducts).ToList();

            subProductList.Insert(0, new SubProduct { Id = 0, Name = "Selecione" });

            return Json(new SelectList(subProductList, "Id", "Name"));
        }
        public JsonResult GetNetworkTypeList(string productName)
        {
            List<LicenseNetworkType> networkTypeList = new();
            switch (productName)
            {
                case "QForm":
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 1, NetworkType = "Local" });
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 2, NetworkType = "Flutuante" });
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 3, NetworkType = "Cliente-Servidor" });
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 4, NetworkType = "Cloud" });
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 0, NetworkType = "Selecione" });
                    break;
                case "JMatPro":
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 2, NetworkType = "Flutuante" });
                    break;
                case "OmniCAD":
                case "PamStamp":
                case "Dante":
                    networkTypeList.Insert(0, new LicenseNetworkType { LicenseNetworkTypeId = 1, NetworkType = "Local" });
                    break;
            }


            return Json(new SelectList(networkTypeList, "LicenseNetworkTypeId", "NetworkType"));
        }
        public JsonResult GetLicenseTimeList(string productName)
        {
            List<LicenseTime> licenseTime = new();
            switch (productName)
            {
                case "QForm":
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 1, Time = "Anual" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 2, Time = "Perpétua" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 3, Time = "Manutenção" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 4, Time = "Atualização" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 5, Time = "Cloud" });
                    break;
                case "JMatPro":
                case "PamStamp":
                case "Dante":
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 1, Time = "Anual" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 2, Time = "Perpétua" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 3, Time = "Manutenção" });
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 4, Time = "Atualização" });
                    break;
                case "OmniCAD":
                    licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 1, Time = "Anual" });
                    break;
            }

            licenseTime.Insert(0, new LicenseTime { LicenseTimeId = 0, Time = "Selecione" });

            return Json(new SelectList(licenseTime, "LicenseTimeId", "Time"));
        }
        public JsonResult GetCommercialLicenseList(string productName)
        {
            List<CommercialLicense> commercialLicense = new();
            commercialLicense.Insert(0, new CommercialLicense { Id = 1, Type = "Comercial" });

            switch (productName)
            {
                case "QForm":
                case "JMatPro":
                    commercialLicense.Insert(0, new CommercialLicense { Id = 2, Type = "Acadêmica" });
                    commercialLicense.Insert(0, new CommercialLicense { Id = 0, Type = "Selecione" });
                    break;
            }

            return Json(new SelectList(commercialLicense, "Id", "Type"));
        }

        public async Task<JsonResult> GetProposalProductItems(int opportunityProductId)
        {
            var proposalProduct = await _context.ProposalProducts
                .FirstOrDefaultAsync(o => o.ProposalId == opportunityProductId);
            var category = proposalProduct.Product.Category.CategoryName;
            var product = proposalProduct.Product.Name;

            return Json($"{category} {product}");
        }
    }

}
