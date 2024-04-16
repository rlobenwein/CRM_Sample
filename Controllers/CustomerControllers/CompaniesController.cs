using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RLBW_ERP.Common;
using RLBW_ERP.Data;
using RLBW_ERP.Models.CustomerModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RLBW_ERP.Controllers.CustomerControllers
{
    [Authorize(Roles = "SuperAdmin,Commercial")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string BIND_STRING = "Id," +
                    "FriendlyName," +
                    "CompanyName," +
                    "TaxpayerNumber," +
                    "MainAddress," +
                    "AddressNumber," +
                    "AddressComplement," +
                    "AddressDistrict," +
                    "PostalCode," +
                    "CityId," +
                    "Id," +
                    "CountryId," +
                    "CompanyPhone," +
                    "CompanyEmail," +
                    "FinanceEmail," +
                    "Status," +
                    "Website," +
                    "Notes";

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Companies
        public async Task<IActionResult> Index(
            bool? active,
            string sortOrder,
            string searchString,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["FriendlyNameSortParm"] = sortOrder == "FriendlyName" ? "FriendlyName_desc" : "FriendlyName";
            ViewData["CompanyNameSortParm"] = sortOrder == "CompanyName" ? "CompanyName_desc" : "CompanyName";
            ViewData["CitySortParm"] = sortOrder == "City" ? "City_desc" : "City";
            ViewData["StateSortParm"] = sortOrder == "State" ? "State_desc" : "State";
            ViewData["CountrySortParm"] = sortOrder == "Country" ? "Country_desc" : "Country";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "Status_desc" : "Status";

            if (pageSize == null)
            {
                pageSize = 25;
            }

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (active == null)
            {
                active = false;
            }

            ViewData["Active"] = active;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentPageSize"] = pageSize;

            var company = _context.Companies
                .Include(a => a.City)
                .Include(o => o.Opportunities)
                .OrderBy(c => c.FriendlyName)
                .AsNoTracking();

            if ((bool)active)
            {
                company = company.Where(o => o.Status.Equals(true));
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                company = company.Where(
                    a => a.FriendlyName.Contains(searchString));
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Date_desc";
            }

            company = sortOrder switch
            {
                "FriendlyName" => company.OrderBy(s => s.FriendlyName),
                "FriendlyName_desc" => company.OrderByDescending(s => s.FriendlyName),
                "CompanyName" => company.OrderBy(s => s.CompanyName),
                "CompanyName_desc" => company.OrderByDescending(s => s.CompanyName),
                "City" => company.OrderBy(s => s.City.Name),
                "City_desc" => company.OrderByDescending(s => s.City.Name),
                "Status" => company.OrderBy(s => s.Status),
                "Status_desc" => company.OrderByDescending(s => s.Status),
                _ => company.OrderBy(s => s.FriendlyName),
            };
            return View(await PaginetedList<Company>.CreateAsync(company.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.City)
                .Include(c => c.Opportunities)
                    .ThenInclude(o => o.Manager)
                .Include(c => c.Opportunities)
                    .ThenInclude(o => o.Product)
                        .ThenInclude(p => p.Category)
                .Include(c => c.Opportunities)
                .Include(c => c.Opportunities)
                    .ThenInclude(p => p.OpportunityActions)
                .Include(c => c.Opportunities)
                    .ThenInclude(x => x.Proposals)
                .Include(c => c.Opportunities)
                    .ThenInclude(c => c.Pipeline)
                .Include(c => c.SoftwareCompanies)
                .Include(c => c.Employees)
                    .ThenInclude(e => e.Person)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            Company company = new() { Status = true };

            ViewData["Id"] = new SelectList(_context.Cities.OrderBy(s => s.Name), "Id", "Name");

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            ViewData["States"] = new SelectList(_context.States.Include(s => s.Country).OrderBy(s => s.Name).AsEnumerable(), "Id", "Name");
            ViewData["Countries"] = new SelectList(_context.Countries.OrderBy(c => c.Name).AsEnumerable(), "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Pipelines, "Id", "Id");
            ViewData["Status"] = true;
            return View(company);
        }

        // POST: Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] Company company)
        {
            TrimStrings.TrimStringsFunction(company);

            if (company.CityId == 0)
            {
                company.CityId = null;
            }
            try
            {
                if (ModelState.IsValid)
                {
                    company.LastUpdate = DateTime.Now;
                    _context.Add(company);
                    await _context.SaveChangesAsync();

                    var referer = Request.Headers["Referer"].ToString();
                    if (referer != null)
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                var number = (int)ex.InnerException.GetType().GetProperty("Number").GetValue(ex.InnerException);

                if (ex.InnerException != null && (number == 1062))
                {
                    ModelState.AddModelError("", $"CNPJ {company.TaxpayerNumber} já cadastrado. ");
                }
                else
                {
                    ModelState.AddModelError("", "Erro ao salvar as alterações. " +
                         "Tente novamente, se o erro persistir, " +
                         "entre em contato com o Administrador do Sistema");
                }
            }
            ViewData["Id"] = new SelectList(_context.Cities, "Id", "Name", company.CityId);
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            ViewData["Id"] = new SelectList(_context.Pipelines, "Id", "Id");
            ViewData["Status"] = true;

            ViewData["Id"] = new SelectList(_context.Cities, "Id", "Name", company.CityId);

            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind(BIND_STRING)] Company company)
        {
            if (company.CityId == 0)
            {
                company.CityId = null;
            }
            if (id == null || id != company.Id)
            {
                ModelState.AddModelError("", "Erro ao salvar as alterações. " +
                    "Tente novamente, se o erro persistir, " +
                    "entre em contato com o Administrador do Sistema");

                var referer = Request.Headers["Referer"].ToString();
                if (referer != null)
                {
                    return Redirect(referer);
                }

                return RedirectToAction(nameof(Index));
            }
            TrimStrings.TrimStringsFunction(company);

            if (ModelState.IsValid)
            {
                try
                {
                    company.LastUpdate = DateTime.Now;
                    _context.Update(company);
                    await _context.SaveChangesAsync();

                    var referer = Request.Headers["Referer"].ToString();
                    if (referer != null)
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Erro ao salvar as alterações. " +
                    "Tente novamente, se o erro persistir, " +
                    "entre em contato com o Administrador do Sistema");
                }
            }

            ViewData["Id"] = new SelectList(_context.Pipelines, "Id", "Id");
            ViewData["Status"] = true;

            ViewData["Id"] = new SelectList(_context.Cities, "Id", "Name", company.CityId);

            return View(company);
        }

        // GET: Companies/Delete/5
        [Authorize(Roles = "SuperAdmin, Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.City)
                    .ThenInclude(c => c.State)
                        .ThenInclude(s => s.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Director")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer != null)
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }

    }
}
