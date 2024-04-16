using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.CustomerModels;
using RLBW_ERP.Models.SalesModels;

namespace RLBW_ERP.Controllers.SalesControllers
{
    public class SoftwareParamsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string BIND_STRING = "Id,SubproductsListId,LicenseNetworkId,Seats,LicenseTimeId,TimeQuantity,Cores,Tasks,Workbenchs,Id";

        public SoftwareParamsController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: SoftwareParams
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SoftwareParams.Include(s => s.CommercialLicense).Include(s => s.LicenseNetworkType).Include(s => s.LicenseTime).Include(s => s.SubproductsList);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SoftwareParams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var softwareParams = await _context.SoftwareParams
                .Include(s => s.CommercialLicense)
                .Include(s => s.LicenseNetworkType)
                .Include(s => s.LicenseTime)
                .Include(s => s.SubproductsList)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (softwareParams == null)
            {
                return NotFound();
            }

            return View(softwareParams);
        }

        // GET: SoftwareParams/Create
        public IActionResult Create(int id)
        {
            var softwareParams = new SoftwareParams()
            {
                SubproductsListId = id,
                Tasks = 1,
                Cores = 6,
                Workbenchs = 0,
                LicenseTimeId = 1,
                LicenseNetworkId = 1,
                CommercialLicenseId = 1,
                TimeQuantity = 1,
                Seats = 1
            };

            ViewData["SubproductName"] = _context.SubproductsLists.Include(x => x.SubProduct).FirstOrDefault().SubProduct.Name;
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type");
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType");
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time");
            ViewData["SubproductsListId"] = new SelectList(_context.SubproductsLists, "Id", "Id");
            return PartialView(softwareParams);
        }

        // POST: SoftwareParams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] SoftwareParams softwareParams)
        {
            if (ModelState.IsValid)
            {
                var calcPrices = new Prices(_cache);
                Opportunity opportunity = await calcPrices.CalcOpportunityValueAsync(softwareParams, _context, false);

                _context.Update(opportunity);
                _context.Add(softwareParams);

                await _context.SaveChangesAsync();

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type", softwareParams.CommercialLicenseId);
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType", softwareParams.LicenseNetworkId);
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time", softwareParams.LicenseTimeId);
            ViewData["SubproductsListId"] = new SelectList(_context.SubproductsLists, "Id", "Id", softwareParams.SubproductsListId);
            return View(softwareParams);
        }

        // GET: SoftwareParams/Edit/5
        public async Task<IActionResult> Edit(int? subproductsListId)
        {
            if (subproductsListId == null)
            {
                return NotFound();
            }

            var softwareParams = await _context.SoftwareParams.Where(x => x.SubproductsListId == subproductsListId).Include(x => x.SubproductsList).ThenInclude(x => x.SubProduct).FirstOrDefaultAsync();
            if (softwareParams == null)
            {
                return NotFound();
            }
            ViewData["Name"] = softwareParams.SubproductsList.SubProduct.Name;
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type", softwareParams.CommercialLicenseId);
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType", softwareParams.LicenseNetworkId);
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time", softwareParams.LicenseTimeId);
            ViewData["SubproductsListId"] = new SelectList(_context.SubproductsLists, "Id", "Id", softwareParams.SubproductsListId);
            return PartialView(softwareParams);
        }

        // POST: SoftwareParams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int softwareParamsId, [Bind(BIND_STRING)] SoftwareParams softwareParams)

        {
            if (softwareParamsId != softwareParams.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var calcPrices = new Prices(_cache);
                Opportunity opportunity = await calcPrices.CalcOpportunityValueAsync(softwareParams, _context, false);

                try
                {
                    _context.Update(opportunity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!SoftwareParamsExists(softwareParams.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        foreach (var entry in ex.Entries)
                        {
                            if (entry.Entity is Person)
                            {
                                var proposedValues = entry.CurrentValues;
                                var databaseValues = entry.GetDatabaseValues();

                                foreach (var property in proposedValues.Properties)
                                {
                                    var proposedValue = proposedValues[property];
                                    var databaseValue = databaseValues[property];

                                    // TODO: decide which value should be written to database
                                    // proposedValues[property] = <value to be saved>;
                                    _context.Update(proposedValue);
                                }

                                // Refresh original values to bypass next concurrency check
                                entry.OriginalValues.SetValues(databaseValues);
                            }
                            else
                            {
                                throw new NotSupportedException(
                                    "Don't know how to handle concurrency conflicts for "
                                    + entry.Metadata.Name);
                            }
                        }
                    }
                }
                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
            }
            ViewData["Id"] = new SelectList(_context.CommercialLicenses, "Id", "Type", softwareParams.CommercialLicenseId);
            ViewData["LicenseNetworkId"] = new SelectList(_context.LicenseNetworkTypes, "LicenseNetworkTypeId", "NetworkType", softwareParams.LicenseNetworkId);
            ViewData["LicenseTimeId"] = new SelectList(_context.LicenseTimes, "LicenseTimeId", "Time", softwareParams.LicenseTimeId);
            ViewData["SubproductsListId"] = new SelectList(_context.SubproductsLists, "Id", "Id", softwareParams.SubproductsListId);
            return View(softwareParams);
        }

        // GET: SoftwareParams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var softwareParams = await _context.SoftwareParams
                .Include(s => s.CommercialLicense)
                .Include(s => s.LicenseNetworkType)
                .Include(s => s.LicenseTime)
                .Include(s => s.SubproductsList)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (softwareParams == null)
            {
                return NotFound();
            }

            return View(softwareParams);
        }

        // POST: SoftwareParams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var softwareParams = await _context.SoftwareParams.FindAsync(id);
            var calcPrices = new Prices(_cache);
            Opportunity opportunity = await calcPrices.CalcOpportunityValueAsync(softwareParams, _context, true);
            _context.Update(opportunity);
            _context.SoftwareParams.Remove(softwareParams);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Details", "Opportunities", new { id = opportunity.Id });
        }

        private bool SoftwareParamsExists(int id)
        {
            return _context.SoftwareParams.Any(e => e.Id == id);
        }
    }
}
