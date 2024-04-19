using CRM_Sample.Common;
using CRM_Sample.Data;
using CRM_Sample.Models.CustomerModels;
using CRM_Sample.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRM_Sample.Controllers.CustomerControllers
{
    public class PeopleController : Controller
    {
        private const string BIND_STRING = "Id, FirstName, MiddleName, LastName, TaxpayerNumber, Birthday, LinkedinProfile, CellPhone, HomePhone, Email, MainAddress, AddressNumber, AddressComplement, PostalCode, AddressDistrict, CityId, Notes, Status";
        private readonly ApplicationDbContext _context;
        public PeopleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index(
            bool? active,
            string sortOrder,
            string searchstring,
            string currentFilter,
            int? pageNumber,
            int? pageSize)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "Name" ? "Name_desc" : "Name";
            ViewData["CitySortParm"] = sortOrder == "City" ? "City_desc" : "City";
            ViewData["StateSortParm"] = sortOrder == "State" ? "State_desc" : "State";
            ViewData["CountrySortParm"] = sortOrder == "Country" ? "Country_desc" : "Country";
            ViewData["CompanySortParm"] = sortOrder == "Company" ? "Company_desc" : "Company";
            ViewData["PositionSortParm"] = sortOrder == "Position" ? "Position_desc" : "Position";
            ViewData["DepartmentSortParm"] = sortOrder == "Department" ? "Department_desc" : "Department";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "Status_desc" : "Status";

            if (pageSize == null)
            {
                pageSize = 25;
            }

            if (searchstring != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchstring = currentFilter;
            }

            ViewData["Active"] = active;
            ViewData["CurrentFilter"] = searchstring;
            ViewData["CurrentPageSize"] = pageSize;

            var people = from p in _context.People
                .Include(p => p.CompanyEmployees)
                .Include(p => p.City)
                .AsNoTracking()
                         select p;


            if (active == null)
            {
                active = false;
            }
            if ((bool)active)
            {
                people = people.Where(o => o.Status.Equals(true));
            }


            if (!string.IsNullOrEmpty(searchstring))
            {
                people = people.Where(a =>
                    a.LastName.Contains(searchstring) ||
                    a.FirstName.Contains(searchstring) ||
                    a.MiddleName.Contains(searchstring)
                    );
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Name";
            }

            switch (sortOrder)
            {
                case "Name":
                    people = people.OrderBy(s => s.FirstName).ThenBy(s => s.MiddleName ?? s.LastName).ThenBy(s => s.LastName);
                    break;
                case "Name_desc":
                    people = people.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.MiddleName ?? s.LastName).ThenByDescending(s => s.LastName);
                    break;
                case "City":
                    people = people.OrderBy(s => s.City.Name);
                    break;
                case "City_desc":
                    people = people.OrderByDescending(s => s.City.Name);
                    break;
                case "Status":
                    people = people.OrderBy(s => s.Status);
                    break;
                case "Status_desc":
                    people = people.OrderByDescending(s => s.Status);
                    break;

                default:
                    people = people.OrderBy(s => s.FirstName).ThenBy(s => s.MiddleName).ThenBy(s => s.LastName);
                    break;
            }

            return View(await PaginetedList<Person>.CreateAsync(people.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.People
                .Include(p => p.CompanyEmployees.Where(ce => ce.PersonId == id))
                    .ThenInclude(ce => ce.Company)
                .Include(p => p.CompanyEmployees.Where(ce => ce.PersonId == id))
                    .ThenInclude(ce => ce.Company)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            Person person = new()
            {
                Status = true
            };
            ViewData["CityId"] = new SelectList(_context.Cities.OrderBy(s => s.Name), "Id", "Name");

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            ViewData["States"] = new SelectList(_context.States.Include(s => s.Country).OrderBy(s => s.Name).AsEnumerable(), "Id", "Name");
            ViewData["Countries"] = new SelectList(_context.Countries.OrderBy(c => c.Name).AsEnumerable(), "Id", "Name");
            ViewData["PipelineId"] = new SelectList(_context.Pipelines, "Id", "Id");
            ViewData["Status"] = true;

            return View(person);
        }

        public async Task<IActionResult> CreateInCompany(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }
            Person person = new()
            {
                Status = true
            };
            ViewData["CityId"] = new SelectList(_context.Cities.OrderBy(s => s.Name), "Id", "Name");

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            ViewData["States"] = new SelectList(_context.States.Include(s => s.Country).OrderBy(s => s.Name).AsEnumerable(), "Id", "Name");
            ViewData["Countries"] = new SelectList(_context.Countries.OrderBy(c => c.Name).AsEnumerable(), "Id", "Name");
            ViewData["PipelineId"] = new SelectList(_context.Pipelines, "Id", "Id");
            ViewData["Status"] = true;

            return PartialView(new PersonDetailsViewModel
            {
                Person = person,
                Company = company
            });
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(BIND_STRING)] Person person)
        {
            TrimStrings.TrimStringsFunction(person);
            if (person.CityId == 0)
            {
                person.CityId = null;
            }

            if (ModelState.IsValid)
            {
                person.LastUpdate = DateTime.Now;
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInCompany(
            [Bind(BIND_STRING)] Person person,[Bind("Id, FriendlyName")] Company company)
        {

            TrimStrings.TrimStringsFunction(person);
            if (person.CityId == 0)
            {
                person.CityId = null;
            }
            company = await _context.Companies.FindAsync(company.Id);

            if (ModelState.IsValid)
            {
                CompanyEmployee employee = new()
                {
                    Person = person,
                    Company = company,
                    LastUpdate=new DateTimeFunctions().GetNow(),
                    Status=true,
                    InitialDate=DateTime.Today,
                };
                person.LastUpdate = new DateTimeFunctions().GetNow();
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Companies", new {id=company.Id});
            }
            return PartialView(new PersonDetailsViewModel
            {
                Person = person,
                Company = company
            });
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["States"] = new SelectList(_context.States.Include(s => s.Country).OrderBy(s => s.Name).AsEnumerable(), "Id", "Name");
            ViewData["Countries"] = new SelectList(_context.Countries.OrderBy(c => c.Name).AsEnumerable(), "Id", "Name");
            ViewData["PipelineId"] = new SelectList(_context.Pipelines, "Id", "Id");
            ViewData["Status"] = true;

            ViewData["CityId"] = new SelectList(_context.Cities.OrderBy(s => s.Name), "Id", "Name");

            GetUrl url = new(HttpContext);
            ViewData["Url"] = url.GetCurrentUrl();

            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(BIND_STRING)] Person person)
        {
            TrimStrings.TrimStringsFunction(person);

            if (id != person.Id)
            {
                return NotFound();
            }
            if (person.CityId == 0)
            {
                person.CityId = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    person.LastUpdate = DateTime.Now;
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Erro ao salvar as alterações. " +
                             "Tente novamente, se o erro persistir, " +
                             "entre em contato com o Administrador do Sistema");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        // GET: Customers/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.People
                .Include(c => c.CompanyEmployees)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Customers/Delete/5
        [Authorize(Roles = "SuperAdmin,Director")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.People.FindAsync(id);
            _context.People.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.Id == id);
        }
    }
}
